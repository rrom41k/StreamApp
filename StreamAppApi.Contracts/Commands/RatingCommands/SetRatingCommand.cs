namespace StreamAppApi.Contracts.Commands.RatingCommands;

public record SetRatingCommand(string movieId, double value);