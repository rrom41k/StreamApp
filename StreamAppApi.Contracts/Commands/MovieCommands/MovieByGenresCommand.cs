namespace StreamAppApi.Contracts.Commands.MovieCommands;

public record MovieByGenresCommand(List<string> genreIds);