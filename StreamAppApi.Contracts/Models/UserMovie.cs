namespace StreamAppApi.Contracts.Models;

public class UserMovie
{
    public string UserId { get; set; }
    public User User { get; set; }
    
    public string MovieId { get; set; }
    public Movie Movie { get; set; }
}