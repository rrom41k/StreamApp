using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

using StreamAppApi.Bll.DbConfiguration;
using StreamAppApi.Contracts.Commands.AuthCommands;
using StreamAppApi.Contracts.Dto;
using StreamAppApi.Contracts.Interfaces;
using StreamAppApi.Contracts.Models;

namespace StreamAppApi.Bll;

public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;
    private readonly StreamPlatformDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthService(
        IConfiguration configuration,
        StreamPlatformDbContext dbContext,
        IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
    }

    public async Task<ResultAuthDto> RegisterUser(
        AuthRegisterCommand authRegisterCommand,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested) // Проверка на отмену запроса
        {
            throw new OperationCanceledException();
        }

        var userContains = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == 
            authRegisterCommand.email, cancellationToken);
        if (userContains != null)
        {
            throw new("User with this email contains in DB");
        }
        
        CreatePasswordHash(authRegisterCommand.password, out var passwordHash, out var passwordSalt);
        User newUser = new(authRegisterCommand.email, passwordHash, passwordSalt);

        await _dbContext.Users.AddAsync(newUser, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        IssueTokenPair(newUser, out var accessToken, out var refreshToken);

        return CreateResult(newUser, accessToken, refreshToken);
    }

    public async Task<ResultAuthDto> LoginUser(
        AuthLoginCommand authLoginCommandCommand,
        CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(
            user => user.Email == authLoginCommandCommand.email,
            cancellationToken) ?? throw new AuthenticationException("User not found");

        if (!VerifyPasswordHash(authLoginCommandCommand.password, user.PasswordHash, user.PasswordSalt))
        {
            throw new AuthenticationException("Wrong password!");
        }

        IssueTokenPair(user, out var accessToken, out var refreshToken);

        return CreateResult(user, accessToken, refreshToken);
    }

    public async Task<ResultAuthDto> GetNewTokens(CancellationToken cancellationToken = default)
    {
        // Получаем refresh токен из запроса или из Cookie, если ничего не было передано в body
        //string? refreshToken = getNewTokensCommand?.refreshToken ?? _httpContextAccessor.HttpContext?.Request.Cookies["refreshToken"]; 
        var refreshToken = _httpContextAccessor.HttpContext?.Request.Cookies["refreshToken"];

        if (refreshToken == null)
        {
            throw new UnauthorizedAccessException("Please, sign in!"); // Если его там нет просим авторизироваться
        }

        var result = ValidateToken(refreshToken); // Проверяем целостность токена, и получаем данные из него

        if (result == null)
        {
            throw new UnauthorizedAccessException("Invalid token or expired!");
        }

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(
                user => user.UserId == result.FindFirst("_id").Value,
                cancellationToken); // Ищем пользователя с _id указанным в токене

        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid Token.");
        }

        if (user.TokenExpires < DateTime.UtcNow)
        {
            throw new InvalidDataException("Token expired."); // Если срок действия токена истёк выдаём ошибку
        }

        IssueTokenPair(user, out var accessToken, out var newRefreshToken);

        return CreateResult(user, accessToken, newRefreshToken);
    }

    private ResultAuthDto CreateResult(User user, string accessToken, string refreshToken)
    {
        var userDto = new UserDto(user.UserId, user.Email, user.IsAdmin);

        return new(userDto, accessToken, refreshToken);
    }

    private void IssueTokenPair(User user, out string accessToken, out string refreshToken)
    {
        accessToken = CreateToken(user, DateTime.UtcNow.AddHours(1));
        refreshToken = GenerateRefreshToken(user);
    }

    private string GenerateRefreshToken(User user)
    {
        user.RefreshToken = CreateToken(user, DateTime.UtcNow.AddDays(15));
        user.TokenCreated = DateTime.UtcNow;
        user.TokenExpires = DateTime.UtcNow.AddDays(15);

        _dbContext.SaveChangesAsync();

        // Set refreshToken in Cookies
        var cookieOptions = new CookieOptions { HttpOnly = true, Expires = DateTime.UtcNow.AddDays(15) };
        _httpContextAccessor.HttpContext?.Response.Cookies.Append("refreshToken", user.RefreshToken, cookieOptions);

        return user.RefreshToken;
    }

    private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
        using var hmac = new HMACSHA512();
        passwordSalt = hmac.Key;
        passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
    }

    private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
    {
        using var hmac = new HMACSHA512(passwordSalt);
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

        return computedHash.SequenceEqual(passwordHash);
    }

    private string CreateToken(User user, DateTime expire)
    {
        var claims = new List<Claim>
        {
            new("_id", user.UserId),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["AppSettings:Token"]));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: expire,
            signingCredentials: creds);

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        return jwt;
    }

    private ClaimsPrincipal ValidateToken(string? token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration["AppSettings:Token"]);

        try
        {
            var claimsPrincipal = tokenHandler.ValidateToken(
                token,
                new()
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false, // При необходимости проверить издателя (Issuer)
                    ValidateAudience = false // При необходимости проверить аудиторию (Audience)
                },
                out var validatedToken);

            return claimsPrincipal; // Токен валиден
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);

            throw; // Токен невалиден
        }
    }
}