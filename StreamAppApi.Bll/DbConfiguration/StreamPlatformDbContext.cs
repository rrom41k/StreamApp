using Microsoft.EntityFrameworkCore;

using StreamAppApi.Contracts.Models;

namespace StreamAppApi.Bll.DbConfiguration;

public class StreamPlatformDbContext : DbContext //IdentityDbContext<ApplicationUser, IdentityRole<string>, string>
{
    public StreamPlatformDbContext(DbContextOptions<StreamPlatformDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Genre> Genres { get; set; }
    public DbSet<Actor> Actors { get; set; }
    public DbSet<Movie> Movies { get; set; }
    public DbSet<MovieParameter> Parameters { get; set; }
    public DbSet<GenreMovie> GenreMovies { get; set; }
    public DbSet<ActorMovie> ActorMovies { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(
            entity =>
            {
                entity.ToTable("Users");
                entity.HasIndex(user => user.Email).IsUnique();
            });
        // Многие ко многим User & Movie (Favorites)
        modelBuilder.Entity<UserMovie>(
            entity => { entity.HasKey(userMovie => new { userMovie.UserId, userMovie.MovieId }); });

        modelBuilder.Entity<UserMovie>(
            entity =>
            {
                entity
                    .HasOne(userMovie => userMovie.User)
                    .WithMany(user => user.Favorites)
                    .HasForeignKey(userMovie => userMovie.UserId)
                    .OnDelete(DeleteBehavior.Cascade);;
            });

        modelBuilder.Entity<UserMovie>(
            entity =>
            {
                entity
                    .HasOne(userMovie => userMovie.Movie)
                    .WithMany(movie => movie.Users)
                    .HasForeignKey(userMovie => userMovie.MovieId)
                    .OnDelete(DeleteBehavior.Cascade);;
            });

        ///////////////////////////

        modelBuilder.Entity<Genre>(
            entity =>
            {
                entity.ToTable("Genres");
                entity.HasIndex(genre => genre.Slug).IsUnique();
            });
        // Многие ко многим Genre & Movie
        modelBuilder.Entity<GenreMovie>(
            entity => { entity.HasKey(genreMovie => new { genreMovie.GenreId, genreMovie.MovieId }); });

        modelBuilder.Entity<GenreMovie>(
            entity =>
            {
                entity
                    .HasOne(genreMovie => genreMovie.Genre)
                    .WithMany(genre => genre.Movies)
                    .HasForeignKey(genreMovie => genreMovie.GenreId)
                    .OnDelete(DeleteBehavior.Cascade);;
            });

        modelBuilder.Entity<GenreMovie>(
            entity =>
            {
                entity
                    .HasOne(genreMovie => genreMovie.Movie)
                    .WithMany(movie => movie.Genres)
                    .HasForeignKey(genreMovie => genreMovie.MovieId)
                    .OnDelete(DeleteBehavior.Cascade);;
            });

        /////////////////////////////////////

        modelBuilder.Entity<Actor>(
            entity =>
            {
                entity.ToTable("Actors");
                entity.HasIndex(actor => actor.Slug).IsUnique();
            });
        // Многие ко многим Actor & Movie
        modelBuilder.Entity<ActorMovie>(
            entity => { entity.HasKey(genreMovie => new { genreMovie.ActorId, genreMovie.MovieId }); });

        modelBuilder.Entity<ActorMovie>(
            entity =>
            {
                entity
                    .HasOne(genreMovie => genreMovie.Actor)
                    .WithMany(actor => actor.Movies)
                    .HasForeignKey(actorMovie => actorMovie.ActorId)
                    .OnDelete(DeleteBehavior.Cascade);;
            });

        modelBuilder.Entity<ActorMovie>(
            entity =>
            {
                entity
                    .HasOne(actorMovie => actorMovie.Movie)
                    .WithMany(movie => movie.Actors)
                    .HasForeignKey(actorMovie => actorMovie.MovieId)
                    .OnDelete(DeleteBehavior.Cascade);;
            });

        ////////////////////////////////////////

        modelBuilder.Entity<Movie>(
            entity =>
            {
                entity.ToTable("Movies");
                entity.HasIndex(movie => movie.Slug).IsUnique();
                entity.HasOne(movie => movie.Parameters)
                    .WithOne(parameter => parameter.Movie)
                    .HasForeignKey<MovieParameter>(parameter => parameter.ParameterId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
    }
}