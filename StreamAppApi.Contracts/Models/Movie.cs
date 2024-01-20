using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using NUlid;

namespace StreamAppApi.Contracts.Models;

public class Movie
{
    public Movie(
        string poster,
        string bigPoster,
        string title,
        string videoUrl,
        string slug,
        double? rating,
        int? countOpened,
        bool? isSendTelegram)
    {
        MovieId = Convert.ToString(Ulid.NewUlid());
        Poster = poster;
        BigPoster = bigPoster;
        Title = title;
        VideoUrl = videoUrl;
        Slug = slug;
        Rating = rating;
        CountOpened = countOpened;
        IsSendTelegram = isSendTelegram;
        Users = new HashSet<UserMovie>();
        Genres = new HashSet<GenreMovie>();
        Actors = new HashSet<ActorMovie>();
    }

    [Key]
    [Column("_id")]
    public string MovieId { get; set; }

    [Required(ErrorMessage = "Poster is required")]
    [Column("poster")]
    public string Poster { get; set; }

    [Required(ErrorMessage = "BigPoster is required")]
    [Column("bigPoster")]
    public string BigPoster { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [Column("title")]
    public string Title { get; set; }

    public MovieParameter Parameters { get; set; }

    [Required(ErrorMessage = "VideoUrl date is required")]
    [Column("videoUrl")]
    public string VideoUrl { get; set; }

    [Required(ErrorMessage = "Release date is required")]
    [Column("slug")]
    public string Slug { get; set; }

    [Column("rating")]
    [DefaultValue(4.0)]
    public double? Rating { get; set; }

    [Column("countOpened")]
    [DefaultValue(0)]
    public int? CountOpened { get; set; }

    [Column("isSendTelegram")]
    public bool? IsSendTelegram { get; set; }

    public ICollection<UserMovie> Users { get; set; }
    public ICollection<GenreMovie> Genres { get; set; }

    public ICollection<ActorMovie> Actors { get; set; }
}