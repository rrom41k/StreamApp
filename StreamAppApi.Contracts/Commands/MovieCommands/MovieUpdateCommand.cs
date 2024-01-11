using StreamAppApi.Contracts.Dto;

namespace StreamAppApi.Contracts.Commands.MovieCommands;

public record MovieUpdateCommand(
    string? poster,
    string? bigPoster,
    string? title,
    string? videoUrl,
    string? slug,
    ParameterDto? parameters,
    double? rating,
    int? countOpened,
    bool? isSendTelegram);