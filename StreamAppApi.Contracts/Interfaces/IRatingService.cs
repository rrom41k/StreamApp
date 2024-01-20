using StreamAppApi.Contracts.Commands.RatingCommands;
using StreamAppApi.Contracts.Dto;

namespace StreamAppApi.Contracts.Interfaces;

public interface IRatingService
{
    Task<RatingDto> SetRating(string userId, SetRatingCommand setRatingCommand, CancellationToken cancellationToken);
    Task<double> GetMovieValueByUser(string userId, string movieId, CancellationToken cancellationToken);
}