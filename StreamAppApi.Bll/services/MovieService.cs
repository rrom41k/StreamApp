using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

using StreamAppApi.Bll.DbConfiguration;
using StreamAppApi.Contracts.Commands.MovieCommands;
using StreamAppApi.Contracts.Dto;
using StreamAppApi.Contracts.Interfaces;
using StreamAppApi.Contracts.Models;

namespace StreamAppApi.Bll;

public class MovieService : IMovieService
{
    private readonly StreamPlatformDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly TelegramBotService _telegramBotService;

    public MovieService(
        StreamPlatformDbContext dbContext,
        IHttpContextAccessor httpContextAccessor,
        TelegramBotOptions telegramBotOptions)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
        _telegramBotService = new(telegramBotOptions);
    }

    public async Task<MovieDto> GetMovieBySlug(
        string slug,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }

        var existingMovie = await _dbContext.Movies.AsNoTracking()
                .Include(movie => movie.Users)
                .Include(movie => movie.Parameters)
                .Include(movie => movie.Actors)
                .ThenInclude(actorMovie => actorMovie.Actor)
                .Include(movie => movie.Genres)
                .ThenInclude(genreMovie => genreMovie.Genre)
                .FirstOrDefaultAsync(existingMovie => existingMovie.Slug == slug, cancellationToken)
            ?? throw new ArgumentException("Movie not found.");

        return MovieToDto(existingMovie);
    }

    public async Task<List<MovieDto>> GetMovieByActor(
        string actorId,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }

        var existingMovies = await _dbContext.Movies.AsNoTracking()
                .Where(
                    existingMovies => existingMovies.Actors
                        .Any(a => a.ActorId == actorId))
                .Include(movie => movie.Users)
                .Include(movie => movie.Parameters)
                .Include(movie => movie.Actors)
                .ThenInclude(actorMovie => actorMovie.Actor)
                .Include(movie => movie.Genres)
                .ThenInclude(genreMovie => genreMovie.Genre)
                .OrderByDescending(movie => movie.Rating)
                .ToListAsync(cancellationToken)
            ?? throw new ArgumentException("Movie not found.");

        return MapMoviesToDto(existingMovies);
    }

    public async Task<List<MovieDto>> GetMovieByGenres(
        MovieByGenresCommand genreIds,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }

        var existingMovies = await _dbContext.Movies.AsNoTracking()
                .Include(movie => movie.Parameters)
                .Include(movie => movie.Actors)
                .ThenInclude(actorMovie => actorMovie.Actor)
                .Include(movie => movie.Genres)
                .ThenInclude(genreMovie => genreMovie.Genre)
                .OrderByDescending(movie => movie.Rating)
                .Where(movie => movie.Genres.Any(genre => genreIds.genreIds.Contains(genre.GenreId)))
                .ToListAsync(cancellationToken)
            ?? throw new ArgumentException("Movie not found.");

        return MapMoviesToDto(existingMovies);
    }

    public async Task<MovieDto> UpdateCountOpenedAsync(
        UpdateCountOpenedCommand updateCountOpenedCommand,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }

        var existingMovie = await _dbContext.Movies
            .Include(movie => movie.Users)
            .Include(movie => movie.Parameters)
            .Include(movie => movie.Actors)
            .ThenInclude(actorMovie => actorMovie.Actor)
            .Include(movie => movie.Genres)
            .ThenInclude(genreMovie => genreMovie.Genre)
            .FirstOrDefaultAsync(m => m.Slug == updateCountOpenedCommand.slug, cancellationToken);

        if (existingMovie != null)
        {
            existingMovie.CountOpened++;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        else
        {
            throw new ArgumentException("Movie not found.");
        }

        return MovieToDto(existingMovie);
    }

    public async Task<List<MovieDto>> GetAllMovies(CancellationToken cancellationToken = default)
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

        var movies = await _dbContext.Movies
            .Where(
                movie =>
                    movie.Title.Contains(searchTerm) || movie.Slug.Contains(searchTerm))
            .AsNoTracking()
            .Include(movie => movie.Users)
            .Include(movie => movie.Parameters)
            .Include(movie => movie.Actors)
            .ThenInclude(actorMovie => actorMovie.Actor)
            .Include(movie => movie.Genres)
            .ThenInclude(genreMovie => genreMovie.Genre)
            .OrderByDescending(movie => movie.Rating)
            .ToListAsync(cancellationToken);

        return MapMoviesToDto(movies);
    }

    /* Admin Rights */

    public async Task<MovieDto> CreateMovie(
        MovieCreateCommand movieCreateCommand,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested) // Проверка на отмену запроса
        {
            throw new OperationCanceledException();
        }

        var findMovie =
            await _dbContext.Movies
                .Include(movie => movie.Users)
                .Include(movie => movie.Parameters)
                .Include(movie => movie.Actors)
                .ThenInclude(actorMovie => actorMovie.Actor)
                .Include(movie => movie.Genres)
                .ThenInclude(genreMovie => genreMovie.Genre)
                .FirstOrDefaultAsync(
                    movie => movie.Slug == movieCreateCommand.slug.ToLower(),
                    cancellationToken);

        if (findMovie != null)
        {
            throw new("Movie with this slug contains in DB");
        }

        var newMovie = CreateMovieHelper(movieCreateCommand);

        _dbContext.Parameters.Add(newMovie.Parameters);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _telegramBotService.SendPostLink(newMovie.Title, newMovie.Poster);

        return MovieToDto(newMovie);
    }

    public async Task<MovieDto> GetMovieById(
        string id,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }

        var existingMovie = await _dbContext.Movies.AsNoTracking()
                .Include(movie => movie.Users)
                .Include(movie => movie.Parameters)
                .Include(movie => movie.Actors)
                .ThenInclude(actorMovie => actorMovie.Actor)
                .Include(movie => movie.Genres)
                .ThenInclude(genreMovie => genreMovie.Genre)
                .FirstOrDefaultAsync(
                    existingMovie =>
                        existingMovie.MovieId == id,
                    cancellationToken)
            ?? throw new ArgumentException("Movie not found.");

        return MovieToDto(existingMovie);
    }

    public async Task<MovieDto> UpdateMovie(
        string id,
        MovieUpdateCommand movieUpdateCommand,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }

        var movieToUpdate = await _dbContext.Movies
            .Include(movie => movie.Users)
            .Include(movie => movie.Parameters)
            .Include(movie => movie.Actors)
            .ThenInclude(actorMovie => actorMovie.Actor)
            .Include(movie => movie.Genres)
            .ThenInclude(genreMovie => genreMovie.Genre)
            .FirstOrDefaultAsync(movieToUpdate => movieToUpdate.MovieId == id, cancellationToken);

        if (movieToUpdate == null)
        {
            throw new ArgumentException("Movie not found.");
        }

        UpdateMovieHelper(ref movieToUpdate, movieUpdateCommand);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MovieToDto(movieToUpdate);
    }

    public async Task<MovieDto> DeleteMovie(
        string id,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }

        var existingMovie = await _dbContext.Movies.AsNoTracking()
            .Include(movie => movie.Parameters)
            .Include(movie => movie.Actors)
            .ThenInclude(actorMovie => actorMovie.Actor)
            .Include(movie => movie.Genres)
            .ThenInclude(genreMovie => genreMovie.Genre)
            .FirstOrDefaultAsync(existingMovie => existingMovie.MovieId == id, cancellationToken);

        if (existingMovie == null)
        {
            throw new ArgumentException("Movie not fount.");
        }

        _dbContext.Movies.Remove(existingMovie);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MovieToDto(existingMovie);
    }

    public async Task<List<MovieDto>> GetMostPopularAsync(CancellationToken cancellationToken = default)
    {
        var popularMovies = await _dbContext.Movies
            .Where(m => m.CountOpened > 0)
            .OrderByDescending(m => m.CountOpened)
            .Include(movie => movie.Users)
            .Include(movie => movie.Parameters)
            .Include(movie => movie.Actors)
            .ThenInclude(actorMovie => actorMovie.Actor)
            .Include(movie => movie.Genres)
            .ThenInclude(genreMovie => genreMovie.Genre)
            .OrderByDescending(movie => movie.Rating)
            .ToListAsync(cancellationToken);

        return MapMoviesToDto(popularMovies);
    }

    // Helpful methods

    private Movie CreateMovieHelper(MovieCreateCommand movieCreateCommand)
    {
        Movie newMovie = new(
            movieCreateCommand.poster,
            movieCreateCommand.bigPoster,
            movieCreateCommand.title,
            movieCreateCommand.videoUrl,
            movieCreateCommand.slug,
            movieCreateCommand.rating ?? 4.0,
            movieCreateCommand.countOpened ?? 0,
            movieCreateCommand.isSendTelegram ?? false) { Parameters = DtoToParameters(movieCreateCommand.parameters) };
        newMovie.Genres = MapGenresArrToList(newMovie, movieCreateCommand.genres);
        newMovie.Actors = MapActorsArrToList(newMovie, movieCreateCommand.actors);
        _dbContext.Movies.Add(newMovie);

        return newMovie;
    }

    private List<GenreMovie> MapGenresArrToList(Movie movie, string[] genres)
    {
        var listGenres = new List<GenreMovie>();

        foreach (var genreId in genres)
        {
            var newGenreMovie = new GenreMovie
            {
                Movie = movie,
                //MovieId = movie.MovieId,
                Genre = _dbContext.Genres.FirstOrDefault(genre => genre.GenreId.Contains(genreId))
                    ?? throw new ArgumentException("Movie with this Id don't exist")
                //GenreId = genreId
            };
            _dbContext.GenreMovies.Add(newGenreMovie);
            listGenres.Add(newGenreMovie);
        }

        return listGenres;
    }

    private List<ActorMovie> MapActorsArrToList(Movie movie, string[] actors)
    {
        var listActors = new List<ActorMovie>();

        foreach (var actorId in actors)
        {
            var newActorMovie = new ActorMovie
            {
                //MovieId = movie.MovieId, 
                Movie = movie,
                //ActorId = actorId
                Actor = _dbContext.Actors.First(actor => actor.ActorId.Contains(actorId))
                    ?? throw new ArgumentException("Not found Actor")
            };
            _dbContext.ActorMovies.Add(newActorMovie);
            listActors.Add(newActorMovie);
        }

        return listActors;
    }

    private void UpdateMovieHelper(ref Movie movieToUpdate, MovieUpdateCommand movieUpdateCommand)
    {
        movieToUpdate.Poster = string.IsNullOrEmpty(movieUpdateCommand.poster) ? movieToUpdate.Poster 
            : movieUpdateCommand.poster;
        
        movieToUpdate.BigPoster = string.IsNullOrEmpty(movieUpdateCommand.bigPoster) ? movieToUpdate.BigPoster
            : movieUpdateCommand.bigPoster;
        
        movieToUpdate.Title = string.IsNullOrEmpty(movieUpdateCommand.title) ? movieToUpdate.Title 
            : movieUpdateCommand.title;
        
        movieToUpdate.VideoUrl = string.IsNullOrEmpty(movieUpdateCommand.videoUrl) ? movieToUpdate.VideoUrl
            : movieUpdateCommand.videoUrl;
        
        movieToUpdate.Slug = string.IsNullOrEmpty(movieUpdateCommand.slug) ? movieToUpdate.Slug 
            : movieUpdateCommand.slug.ToLower();
        
        movieToUpdate.Parameters = movieUpdateCommand.parameters == null ? movieToUpdate.Parameters 
            : DtoToParameters(movieUpdateCommand.parameters);
        
        movieToUpdate.Rating = movieUpdateCommand.rating ?? movieToUpdate.Rating;
        
        movieToUpdate.CountOpened = movieUpdateCommand.countOpened ?? movieToUpdate.CountOpened;
        
        movieToUpdate.IsSendTelegram = movieUpdateCommand.isSendTelegram ?? movieToUpdate.IsSendTelegram;
    }

    static ParameterDto ParametersToDto(MovieParameter parameters)
    {
        return new(parameters.Year, parameters.Duration, parameters.Country);
    }

    static MovieParameter DtoToParameters(ParameterDto parametersDto)
    {
        return new(parametersDto.year, parametersDto.duration, parametersDto.country);
    }

    static MovieDto MovieToDto(Movie movie)
    {
        return new(
            movie.MovieId,
            movie.Poster,
            movie.BigPoster,
            movie.Title,
            movie.VideoUrl,
            movie.Slug,
            ParametersToDto(movie.Parameters),
            GenreService.MapGenresToDto(movie.Genres),
            ActorService.MapActorsToDto(movie.Actors),
            movie.Rating,
            movie.CountOpened,
            movie.IsSendTelegram);
    }

    public static List<MovieDto> MapMoviesToDto(List<Movie> movies)
    {
        List<MovieDto> moviesListDto = new();

        foreach (var movie in movies)
        {
            moviesListDto.Add(MovieToDto(movie));
        }

        return moviesListDto;
    }
}