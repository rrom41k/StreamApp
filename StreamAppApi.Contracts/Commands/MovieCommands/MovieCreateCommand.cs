using StreamAppApi.Contracts.Dto;

namespace StreamAppApi.Contracts.Commands.MovieCommands;

public record MovieCreateCommand(
    string poster,
    string bigPoster,
    string title,
    string videoUrl,
    string slug,
    ParameterDto parameters,
    string[] genres,
    string[] actors,
    double? rating,
    int? countOpened,
    bool? isSendTelegram);