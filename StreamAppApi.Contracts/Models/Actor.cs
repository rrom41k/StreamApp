using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using NUlid;

namespace StreamAppApi.Contracts.Models;

public class Actor
{
    public Actor(string name, string slug, string? photo)
    {
        ActorId = Convert.ToString(Ulid.NewUlid());
        Name = name;
        Slug = slug;
        Photo = photo;
        Movies = new HashSet<ActorMovie>();
    }

    [Key]
    [Column("_id")]
    public string ActorId { get; set; }

    [Required(ErrorMessage = "Name is required")]
    [Column("name")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Slug is required")]
    [Column("slug")]
    public string Slug { get; set; }

    [Required(ErrorMessage = "Photo is required")]
    [Column("photo")]
    public string? Photo { get; set; }

    public ICollection<ActorMovie> Movies { get; set; }
}