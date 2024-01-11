using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

using StreamAppApi.Bll.DbConfiguration;
using StreamAppApi.Contracts.Commands.UserCommands;
using StreamAppApi.Contracts.Dto;
using StreamAppApi.Contracts.Interfaces;
using StreamAppApi.Contracts.Models;

namespace StreamAppApi.Bll;

public class UserService : IUserService
{
    private readonly StreamPlatformDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserService(StreamPlatformDbContext dbContext, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<UserDto> CreateUser(UserCreateCommand user, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested) // Проверка на отмену запроса
        {
            throw new OperationCanceledException();
        }

        if (!IsValidEmail(user.email)) // Валидация полей
        {
            throw new ArgumentException("Invalid email");
        }

        if (user.password.Length < 6) // Валидация полей
        {
            throw new ArgumentException("Invalid password length");
        }

        CreatePasswordHash(user.password, out var passwordHash, out var passwordSalt);
        User newUser = new(
            user.email,
            passwordHash,
            passwordSalt,
            user.isAdmin);

        if (_dbContext.Users.Contains(newUser))
        {
            throw new("User with this email contains in DB");
        }

        _dbContext.Users.Add(newUser);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var newUserDto = new UserDto(newUser.UserId, newUser.Email, newUser.IsAdmin);

        return newUserDto;
    }

    public async Task<int> GetUsersCount(CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }

        var usersCount = await _dbContext.Users.CountAsync(cancellationToken);

        return usersCount;
    }

    public async Task<List<Dictionary<string, UserDto>>> GetAllUsers(CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }

        string? searchTerm = _httpContextAccessor.HttpContext.Request.Query["searchTerm"];

        if (searchTerm == null)
        {
            searchTerm = "";
        }

        var users = await _dbContext.Users.AsNoTracking()
            .Where(user => user.Email.Contains(searchTerm))
            .ToListAsync(cancellationToken);

        return MapUsersToDTO(users);
    }

    public async Task<UserDto> GetUserById(string id, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }

        var existingUser = await _dbContext.Users.AsNoTracking()
                .FirstOrDefaultAsync(existingUser => existingUser.UserId == id, cancellationToken)
            ?? throw new ArgumentException("User not found.");

        var existingUserDto = new UserDto(existingUser.UserId, existingUser.Email, existingUser.IsAdmin);

        return existingUserDto;
    }

    public async Task<UserDto> UpdateUser(
        string id,
        UserUpdateCommand updateUserData,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }

        var userToUpdate = await _dbContext.Users.AsNoTracking()
            .FirstOrDefaultAsync(userToUpdate => userToUpdate.UserId == id, cancellationToken);

        if (userToUpdate == null)
        {
            throw new ArgumentException("User not found.");
        }

        UpdateUserHelper(userToUpdate, updateUserData);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var updatedUserDto = new UserDto(userToUpdate.UserId, userToUpdate.Email, userToUpdate.IsAdmin);

        return updatedUserDto;
    }

    public async Task<UserDto> DeleteUser(string id, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }

        var existingUser = await _dbContext.Users.AsNoTracking()
            .FirstOrDefaultAsync(existingUser => existingUser.UserId == id, cancellationToken);

        if (existingUser == null)
        {
            throw new ArgumentException("User not fount.");
        }

        var removedUser = new UserDto(existingUser.UserId, existingUser.Email, existingUser.IsAdmin);

        _dbContext.Users.Remove(existingUser);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return removedUser;
    }

    private void UpdateUserHelper(User user, UserUpdateCommand updateUserData)
    {
        CreatePasswordHash(updateUserData.password, out var passwordHash, out var passwordSalt);
        user.Email = updateUserData.email ?? user.Email;
        user.PasswordHash = updateUserData.password == null || updateUserData.password == ""
            ? user.PasswordHash
            : passwordHash;
        user.PasswordSalt = updateUserData.password == null || updateUserData.password == ""
            ? user.PasswordSalt
            : passwordSalt;
        user.IsAdmin = updateUserData.isAdmin ?? user.IsAdmin;
    }

    private List<Dictionary<string, UserDto>> MapUsersToDTO(List<User> users)
    {
        List<Dictionary<string, UserDto>> userListDTO = new();

        foreach (var user in users)
        {
            var userDTO = new UserDto(user.UserId, user.Email, user.IsAdmin);
            var userDict = new Dictionary<string, UserDto> { { "user", userDTO } };
            userListDTO.Add(userDict);
        }

        return userListDTO;
    }

    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        try
        {
            // Normalize the domain
            email = Regex.Replace(
                email,
                @"(@)(.+)$",
                DomainMapper,
                RegexOptions.None,
                TimeSpan.FromMilliseconds(200));

            // Examines the domain part of the email and normalizes it.
            string DomainMapper(Match match)
            {
                // Use IdnMapping class to convert Unicode domain names.
                var idn = new IdnMapping();

                // Pull out and process domain name (throws ArgumentException on invalid)
                var domainName = idn.GetAscii(match.Groups[2].Value);

                return match.Groups[1].Value + domainName;
            }
        }
        catch (RegexMatchTimeoutException e)
        {
            return false;
        }
        catch (ArgumentException e)
        {
            return false;
        }

        try
        {
            return Regex.IsMatch(
                email,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                RegexOptions.IgnoreCase,
                TimeSpan.FromMilliseconds(250));
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }

    private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
        using var hmac = new HMACSHA512();
        passwordSalt = hmac.Key;
        passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
    }
}