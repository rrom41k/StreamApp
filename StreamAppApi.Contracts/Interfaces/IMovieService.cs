using StreamAppApi.Contracts.Commands.MovieCommands;
using StreamAppApi.Contracts.Dto;

namespace StreamAppApi.Contracts.Interfaces;

public interface IMovieService
{
    Task<MovieDto> GetMovieBySlug(string slug, CancellationToken cancellationToken);

    Task<List<MovieDto>> GetMovieByActor(
        string actorId,
        CancellationToken cancellationToken);

    Task<List<MovieDto>> GetMovieByGenres(
        MovieByGenresCommand genreIds,
        CancellationToken cancellationToken);

    Task<MovieDto> UpdateCountOpenedAsync(
        UpdateCountOpenedCommand updateCountOpenedCommand,
        CancellationToken cancellationToken);

    Task<List<MovieDto>> GetAllMovies(CancellationToken cancellationToken);

    /* Admin Rights */

    Task<MovieDto> CreateMovie(
        MovieCreateCommand movieCreateCommand,
        CancellationToken cancellationToken);

    Task<MovieDto> GetMovieById(
        string id,
        CancellationToken cancellationToken);

    Task<MovieDto> UpdateMovie(
        string id,
        MovieUpdateCommand movieUpdateCommand,
        CancellationToken cancellationToken);

    Task<MovieDto> DeleteMovie(
        string id,
        CancellationToken cancellationToken);

    Task<List<MovieDto>> GetMostPopularAsync(CancellationToken cancellationToken);
}