namespace StreamAppApi.Contracts.Commands.AuthCommands;

public record AuthRegisterCommand(string email, string password);