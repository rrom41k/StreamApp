using StreamAppApi.Contracts.Commands.UserCommands;
using StreamAppApi.Contracts.Dto;

namespace StreamAppApi.Contracts.Interfaces;

public interface IUserService
{
    Task<UserDto> CreateUser(UserCreateCommand userCreateCommand, CancellationToken cancellationToken);
    Task<int> GetUsersCount(CancellationToken cancellationToken);
    Task<UserDto> GetUserById(string id, CancellationToken cancellationToken);
    Task<List<Dictionary<string, UserDto>>> GetAllUsers(CancellationToken cancellationToken);
    Task<UserDto> UpdateUser(string id, UserUpdateCommand user, CancellationToken cancellationToken);
    Task<UserDto> DeleteUser(string id, CancellationToken cancellationToken);
}