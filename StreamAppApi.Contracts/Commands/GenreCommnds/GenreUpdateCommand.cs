namespace StreamAppApi.Contracts.Commands.GenreCommands;

public record GenreUpdateCommand(string? name, string? slug, string? description, string? icon);