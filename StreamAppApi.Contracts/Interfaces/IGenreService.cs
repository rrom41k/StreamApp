using StreamAppApi.Contracts.Commands.GenreCommands;
using StreamAppApi.Contracts.Dto;

namespace StreamAppApi.Contracts.Interfaces;

public interface IGenreService
{
    Task<GenreDto> GetGenreBySlug(string slug, CancellationToken cancellationToken);
    Task<List<Dictionary<string, GenreDto>>> GetAllGenres(CancellationToken cancellationToken);
    Task<List<Dictionary<string, GenreDto>>> GetCollections(CancellationToken cancellationToken);
    /* Admin Rights */
    Task<GenreDto> CreateGenre(GenreCreateCommand genreCreateCommand ,CancellationToken cancellationToken);
    Task<GenreDto> GetGenreById(string id, CancellationToken cancellationToken);
    Task<GenreDto> UpdateGenre(string id, GenreUpdateCommand genreUpdateCommand, CancellationToken cancellationToken);
    Task<GenreDto> DeleteGenre(string id, CancellationToken cancellationToken);
}