namespace StreamAppApi.Contracts.Dto;

public record ResultAuthDto(UserDto user, string accessToken, string refreshToken);