using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using NUlid;

namespace StreamAppApi.Contracts.Models;

public class MovieParameter
{
    public MovieParameter(int year, int duration, string country)
    {
        ParameterId = Convert.ToString(Ulid.NewUlid());
        Year = year;
        Duration = duration;
        Country = country;
    }

    [Key]
    [Column("_id")]
    public string ParameterId { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [Column("year")]
    public int Year { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [Column("duration")]
    public int Duration { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [Column("country")]
    public string Country { get; set; }

    public Movie Movie { get; set; }
}