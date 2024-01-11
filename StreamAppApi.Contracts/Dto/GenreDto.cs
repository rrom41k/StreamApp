namespace StreamAppApi.Contracts.Dto;

public record GenreDto(
    string _id,
    string name,
    string slug,
    string description,
    string icon);