namespace StreamAppApi.Contracts.Models;

public class GenreMovie
{
    public string GenreId { get; set; }
    public Genre Genre { get; set; }

    public string MovieId { get; set; }
    public Movie Movie { get; set; }
}