namespace StreamAppApi.Contracts.Dto;

public record MovieDto(
    string movieId, 
    string poster, 
    string bigPoster, 
    string title, 
    string videoUrl, 
    string slug, 
    ParameterDto parameters, 
    double? rating, 
    int? countOpened, 
    bool? isSendTelegram);