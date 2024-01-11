namespace StreamAppApi.Contracts.Models;

public class ActorMovie
{
    public string ActorId { get; set; }
    public Actor Actor { get; set; }

    public string MovieId { get; set; }
    public Movie Movie { get; set; }
}