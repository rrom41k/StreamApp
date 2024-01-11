namespace StreamAppApi.Contracts.Commands.GenreCommands;

public record GenreCreateCommand(
    string name,
    string slug,
    string description,
    string icon);