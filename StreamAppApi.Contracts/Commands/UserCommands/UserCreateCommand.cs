namespace StreamAppApi.Contracts.Commands.UserCommands;

public record UserCreateCommand(string email, string password, bool isAdmin);