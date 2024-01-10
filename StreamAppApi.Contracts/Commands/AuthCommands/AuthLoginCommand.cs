namespace StreamAppApi.Contracts.Commands.AuthCommands;

public record AuthLoginCommand(string email, string password);