namespace StreamAppApi.Contracts.Commands.UserCommands;

public record UserUpdateCommand(string? email, string? password, bool? isAdmin);