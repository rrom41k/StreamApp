namespace StreamAppApi.Contracts.Commands.ActorCommands;

public record ActorCreateCommand(string name, string slug, string photo);