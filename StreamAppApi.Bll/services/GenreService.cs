using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

using StreamAppApi.Bll.DbConfiguration;
using StreamAppApi.Contracts.Commands.GenreCommands;
using StreamAppApi.Contracts.Dto;
using StreamAppApi.Contracts.Interfaces;
using StreamAppApi.Contracts.Models;

namespace StreamAppApi.Bll;

public class GenreService : IGenreService
{
    private readonly StreamPlatformDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GenreService(StreamPlatformDbContext dbContext, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<GenreDto> GetGenreBySlug(string slug, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }

        var existingGenre = await _dbContext.Genres.AsNoTracking()
                .FirstOrDefaultAsync(existingGenre => existingGenre.Slug == slug, cancellationToken)
            ?? throw new ArgumentException("Genre not found.");

        return GenreToDto(existingGenre);
    }

    public async Task<List<GenreDto>> GetAllGenres(CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }

        string? searchTerm = _httpContextAccessor.HttpContext.Request.Query["searchTerm"];

        if (searchTerm == null)
        {
            searchTerm = "";
        }

        var genres = await _dbContext.Genres.AsNoTracking()
            .Where(
                genre =>
                    genre.Name.Contains(searchTerm)
                    || genre.Slug.Contains(searchTerm)
                    || genre.Description.Contains(searchTerm))
            .ToListAsync(cancellationToken);

        return MapGenresToDto(genres);
    }

    public async Task<List<CollectionDto>> GetCollections(CancellationToken cancellationToken)
    {
        var genres = await _dbContext.Genres.AsNoTracking().ToListAsync(cancellationToken);
        var collection = new List<CollectionDto>();

        foreach (var genre in genres)
        {
            var genreMovies = await _dbContext.GenreMovies.AsNoTracking()
                .Include(genreMovie => genreMovie.Movie)
                .FirstOrDefaultAsync(movie => movie.GenreId == genre.GenreId, cancellationToken);

            if (genreMovies == null)
            {
                continue;
            }

            var result = new CollectionDto(
                genre.GenreId,
                genreMovies.Movie.BigPoster,
                genre.Name,
                genre.Slug);

            collection.Add(result);
        }

        return collection;
    }

    /* Admin Rights */

    public async Task<GenreDto> CreateGenre(GenreCreateCommand genreCreateCommand, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested) // Проверка на отмену запроса
        {
            throw new OperationCanceledException();
        }

        var findGenre =
            await _dbContext.Genres.FirstOrDefaultAsync(genre => genre.Slug == genreCreateCommand.slug.ToLower());

        if (findGenre != null)
        {
            throw new("Genre with this slug contains in DB");
        }

        Genre newGenre = new(
            genreCreateCommand.name,
            genreCreateCommand.slug.ToLower(),
            genreCreateCommand.description,
            genreCreateCommand.icon);

        _dbContext.Genres.Add(newGenre);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return GenreToDto(newGenre);
    }

    public async Task<GenreDto> GetGenreById(string id, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }

        var existingGenre = await _dbContext.Genres.AsNoTracking()
                .FirstOrDefaultAsync(existingGenre => existingGenre.GenreId == id, cancellationToken)
            ?? throw new ArgumentException("Genre not found.");

        return GenreToDto(existingGenre);
    }

    public async Task<GenreDto> UpdateGenre(
        string id,
        GenreUpdateCommand genreUpdateCommand,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }

        var genreToUpdate = await _dbContext.Genres.AsNoTracking()
            .FirstOrDefaultAsync(genreToUpdate => genreToUpdate.GenreId == id, cancellationToken);

        if (genreToUpdate == null)
        {
            throw new ArgumentException("Genre not found.");
        }

        UpdateGenreHelper(genreToUpdate, genreUpdateCommand);
        await _dbContext.SaveChangesAsync(cancellationToken);


        return GenreToDto(genreToUpdate);
    }

    public async Task<GenreDto> DeleteGenre(string id, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }

        var existingGenre = await _dbContext.Genres.AsNoTracking()
            .FirstOrDefaultAsync(existingGenre => existingGenre.GenreId == id, cancellationToken);

        if (existingGenre == null)
        {
            throw new ArgumentException("Genre not fount.");
        }

        _dbContext.Genres.Remove(existingGenre);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return GenreToDto(existingGenre);
    }

    // Helpful methods

    private void UpdateGenreHelper(Genre genreToUpdate, GenreUpdateCommand genreUpdateCommand)
    {
        genreToUpdate.Name = string.IsNullOrEmpty(genreUpdateCommand.name) ? genreToUpdate.Name : 
            genreUpdateCommand.name;
        
        genreToUpdate.Slug = string.IsNullOrEmpty(genreUpdateCommand.slug.ToLower()) ? genreToUpdate.Slug : 
            genreUpdateCommand.slug.ToLower();
        
        genreToUpdate.Description = string.IsNullOrEmpty(genreUpdateCommand.description)? genreToUpdate.Description : 
            genreUpdateCommand.description;
        
        genreToUpdate.Icon = string.IsNullOrEmpty(genreUpdateCommand.icon)? genreToUpdate.Icon : genreUpdateCommand.icon;
    }

    public static GenreDto GenreToDto(Genre genre)
    {
        return new(
            genre.GenreId,
            genre.Name,
            genre.Slug,
            genre.Description,
            genre.Icon);
    }

    private List<GenreDto> MapGenresToDto(List<Genre> genres)
    {
        List<GenreDto> genresListDto = new();

        foreach (var genre in genres)
        {
            genresListDto.Add(GenreToDto(genre));
        }

        return genresListDto;
    }

    public static List<GenreDto> MapGenresToDto(ICollection<GenreMovie> genres)
    {
        List<GenreDto> genresListDto = new();

        foreach (var genre in genres)
        {
            var genreDto = GenreToDto(genre.Genre);
            genresListDto.Add(genreDto);
        }

        return genresListDto;
    }
}