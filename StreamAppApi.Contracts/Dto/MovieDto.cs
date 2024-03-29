namespace StreamAppApi.Contracts.Dto;

public record MovieDto(
    string _id,
    string poster,
    string bigPoster,
    string title,
    string videoUrl,
    string slug,
    ParameterDto parameters,
    List<GenreDto> genres,
    List<ActorDto> actors,
    double? rating,
    int? countOpened,
    bool? isSendTelegram);