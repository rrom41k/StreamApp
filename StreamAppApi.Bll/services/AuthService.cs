using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using StreamAppApi.Bll.DbConfiguration;
using StreamAppApi.Contracts.Commands.AuthCommands;
using StreamAppApi.Contracts.Dto;
using StreamAppApi.Contracts.Interfaces;
using StreamAppApi.Contracts.Models;

namespace StreamAppApi.Bll;

public class AuthService : IAuthService
{
    private readonly string _appSetTok;
    private readonly StreamPlatformDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthService(
        string appSettingsToken,
        StreamPlatformDbContext dbContext,
        IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
        _appSetTok = appSettingsToken;
    }

    public async Task<ResultAuthDto> RegisterUser(
        AuthRegisterCommand authRegisterCommand,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested) // Проверка на отмену запроса
        {
            throw new OperationCanceledException();
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
            cancellationToken);

        if (!VerifyPasswordHash(authLoginCommandCommand.password, user.PasswordHash, user.PasswordSalt))
        {
            throw new AuthenticationException("Wrong password!");
        }

        IssueTokenPair(user, out var accessToken, out var refreshToken);

        return CreateResult(user, accessToken, refreshToken);
    }

    //public async Task<ResultAuthDto> GetNewTokens(AuthGetNewTokensCommand? getNewTokensCommand,CancellationToken cancellationToken = default)
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

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appSetTok));

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
        var key = Encoding.UTF8.GetBytes(_appSetTok);

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

/*private RefreshToken GenerateRefreshToken()
{
    var refreshToken = new RefreshToken
    {
        Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
        Expires = DateTime.UtcNow.AddDays(7),
        Created = DateTime.UtcNow
    };
    return refreshToken;
}*/
/*public async Task<object> RefreshToken(CancellationToken cancellationToken = default)
{
    var refreshToken = _httpContextAccessor.HttpContext.Request.Cookies["refreshToken"]; // получаем refreshToken из куки

    var logUser = await _dbContext.Users.FirstOrDefaultAsync(user => user.RefreshToken == refreshToken, cancellationToken); // поиск пользователя с заданным refreshToken

    // Валидация полученных данных
    if (logUser == null)
        throw new UnauthorizedAccessException("Invalid Refresh Token.");
    if (logUser.TokenExpires < DateTime.UtcNow)
        throw new InvalidDataException("Token expired.");

    //генерация новых токенов

    //string accessToken = CreateToken(logUser);
    //var newRefreshToken = GenerateRefreshToken();

    IssueTokenPair(logUser.ActorId, out string accessToken, out RefreshToken newRefreshToken);

    SetRefreshToken(newRefreshToken, logUser);

    await _dbContext.SaveChangesAsync(cancellationToken);

    return new
    {
        user = new UserDto(logUser.ActorId, logUser.Email, logUser.IsAdmin),
        accessToken = accessToken,
        refreshToken = newRefreshToken.Token
    };
}*/