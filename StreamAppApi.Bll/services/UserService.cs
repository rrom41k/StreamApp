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

        var userContains = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == user.email, cancellationToken);
        if (userContains != null)
        {
            throw new("User with this email contains in DB");
        }

        CreatePasswordHash(user.password, out var passwordHash, out var passwordSalt);
        User newUser = new(
            user.email,
            passwordHash,
            passwordSalt,
            user.isAdmin);

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

    public async Task<List<UserDto>> GetAllUsers(CancellationToken cancellationToken = default)
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

    public async Task<List<MovieDto>> GetFavorites(string id, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }

        var data = await _dbContext.UserMovies.AsNoTracking()
                .Include(um => um.Movie)
                .Include(um => um.Movie.Parameters)
                .Include(um => um.Movie.Genres)
                .ThenInclude(gm => gm.Genre)
                .Include(um => um.Movie.Actors)
                .ThenInclude(am => am.Actor)
                .Where(um => um.UserId == id)
                .ToListAsync(cancellationToken)
            ?? throw new ArgumentException("User not found.");

        var favorites = new List<Movie>();

        foreach (var movie in data)
        {
            favorites.Add(movie.Movie);
        }

        return MovieService.MapMoviesToDto(favorites);
    }

    public async Task UpdateFavorites(
        string userId,
        UserFavoritesUpdateCommand userFavoritesUpdateCommand,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }

        var data = await _dbContext.UserMovies
                .Where(um => um.UserId == userId)
                .ToListAsync(cancellationToken)
            ?? throw new ArgumentException("User not found.");

        var checkMovie = data.FirstOrDefault(um => um.MovieId == userFavoritesUpdateCommand.movieId);

        if (checkMovie != null)
        {
            _dbContext.UserMovies.Remove(checkMovie);
        }
        else
        {
            var newFavorMovie = new UserMovie { UserId = userId, MovieId = userFavoritesUpdateCommand.movieId };
            _dbContext.UserMovies.Add(newFavorMovie);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
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

        var userToUpdate = await _dbContext.Users
            .FirstOrDefaultAsync(userToUpdate => userToUpdate.UserId == id, cancellationToken);

        if (userToUpdate == null)
        {
            throw new ArgumentException("User not found.");
        }

        UpdateUserHelper(ref userToUpdate, updateUserData);
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

    private void UpdateUserHelper(ref User user, UserUpdateCommand updateUserData)
    {
        user.Email = string.IsNullOrEmpty(updateUserData.email) ? user.Email : updateUserData.email;
        
        if (!string.IsNullOrEmpty(updateUserData.password))
        {
            CreatePasswordHash(updateUserData.password, out var passwordHash, out var passwordSalt);
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
        }

        user.IsAdmin = updateUserData.isAdmin ?? user.IsAdmin;
    }

    private List<UserDto> MapUsersToDTO(List<User> users)
    {
        List<UserDto> userListDTO = new();

        foreach (var user in users)
        {
            var userDTO = new UserDto(user.UserId, user.Email, user.IsAdmin);
            userListDTO.Add(userDTO);
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