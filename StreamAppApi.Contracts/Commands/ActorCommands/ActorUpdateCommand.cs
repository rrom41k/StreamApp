namespace StreamAppApi.Contracts.Commands.ActorCommands;

public record ActorUpdateCommand(string? name, string? slug, string? photo);