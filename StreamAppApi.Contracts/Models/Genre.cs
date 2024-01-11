using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using NUlid;

namespace StreamAppApi.Contracts.Models;

public class Genre
{
    public Genre(
        string name,
        string slug,
        string? description,
        string icon)
    {
        GenreId = Convert.ToString(Ulid.NewUlid());
        Name = name;
        Slug = slug;
        Description = description;
        Icon = icon;
    }

    [Key]
    [Column("_id")]
    public string GenreId { get; set; }

    [Required(ErrorMessage = "Name is required")]
    [Column("name")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Slug is required")]
    [Column("slug")]
    public string Slug { get; set; }

    [Required(ErrorMessage = "Description is required")]
    [Column("description")]
    public string? Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Icon is required")]
    [Column("icon")]
    public string Icon { get; set; }

    public ICollection<GenreMovie> Movies { get; set; }
}