using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

using StreamAppApi.Bll.DbConfiguration;
using StreamAppApi.Contracts.Commands.RatingCommands;
using StreamAppApi.Contracts.Dto;
using StreamAppApi.Contracts.Interfaces;
using StreamAppApi.Contracts.Models;

namespace StreamAppApi.Bll;

public class RatingService : IRatingService
{
    private readonly StreamPlatformDbContext _dbContext;

    public RatingService(StreamPlatformDbContext dbContext, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
    }

    public async Task<RatingDto> SetRating(
        string userId,
        SetRatingCommand setRatingCommand,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested) // Проверка на отмену запроса
        {
            throw new OperationCanceledException();
        }

        var userMovie = await _dbContext.UserMovies
            .FirstOrDefaultAsync(
                um =>
                    um.MovieId == setRatingCommand.movieId && um.UserId == userId,
                cancellationToken);

        if (userMovie != null)
        {
            userMovie.Rating = setRatingCommand.value;
        }
        else
        {
            userMovie = new()
            {
                UserId = userId,
                MovieId = setRatingCommand.movieId,
                Rating = setRatingCommand.value
            };

            _dbContext.UserMovies.Add(userMovie);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        // Рассчитываем средний рейтинг для фильма.
        var averageRating = await AverageRatingByMovieAsync(setRatingCommand.movieId, cancellationToken);

        // Обновляем средний рейтинг для фильма.
        await UpdateRatingAsync(setRatingCommand.movieId, averageRating, cancellationToken);

        return UserMovieToRatingDto(userMovie);
    }

    public async Task<double> GetMovieValueByUser(
        string userId,
        string movieId,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested) // Проверка на отмену запроса
        {
            throw new OperationCanceledException();
        }

        var rating = await _dbContext.UserMovies
            .Where(um => um.MovieId == movieId && um.UserId == userId)
            .Select(um => um.Rating)
            .FirstOrDefaultAsync(cancellationToken);

        return rating ?? 0;
    }

    private async Task UpdateRatingAsync(
        string movieId,
        double newRating,
        CancellationToken cancellationToken = default)
    {
        var movie = await _dbContext.Movies
            .FirstOrDefaultAsync(m => m.MovieId == movieId, cancellationToken);

        if (movie != null)
        {
            movie.Rating = newRating;
            _dbContext.Movies.Update(movie);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        else
        {
            throw new ArgumentException("Movie not fount.");
        }
    }

    private async Task<double> AverageRatingByMovieAsync(string movieId, CancellationToken cancellationToken)
    {
        var ratingsMovie = await _dbContext.UserMovies
            .Where(um => um.MovieId == movieId)
            .ToListAsync(cancellationToken);

        if (ratingsMovie.Count > 0)
        {
            double? totalRating = 0;
            int? countUsers = 0;

            foreach (var um in ratingsMovie)
            {
                if (um.Rating != null)
                {
                    totalRating += um.Rating;
                    countUsers++;
                }
            }

            return totalRating ?? 0 / countUsers ?? 1;
        }

        // Возвращайте значение по умолчанию, например, 0, если нет рейтингов для данного фильма.
        return 0;
    }

    private RatingDto UserMovieToRatingDto(UserMovie userMovie)
    {
        return new(userMovie.UserId, userMovie.MovieId, userMovie.Rating ?? 0);
    }
}