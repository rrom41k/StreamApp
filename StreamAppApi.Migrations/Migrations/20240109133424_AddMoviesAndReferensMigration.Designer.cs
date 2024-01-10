﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using StreamAppApi.Bll.DbConfiguration;

#nullable disable

namespace StreamAppApi.Migrations.Migrations
{
    [DbContext(typeof(StreamPlatformDbContext))]
    [Migration("20240109133424_AddMoviesAndReferensMigration")]
    partial class AddMoviesAndReferensMigration
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.14")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("StreamAppApi.Contracts.Models.Actor", b =>
                {
                    b.Property<string>("ActorId")
                        .HasColumnType("text")
                        .HasColumnName("_id");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.Property<string>("Photo")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("photo");

                    b.Property<string>("Slug")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("slug");

                    b.HasKey("ActorId");

                    b.HasIndex("Slug")
                        .IsUnique();

                    b.ToTable("Actors", (string)null);
                });

            modelBuilder.Entity("StreamAppApi.Contracts.Models.ActorMovie", b =>
                {
                    b.Property<string>("ActorId")
                        .HasColumnType("text");

                    b.Property<string>("MovieId")
                        .HasColumnType("text");

                    b.HasKey("ActorId", "MovieId");

                    b.HasIndex("MovieId");

                    b.ToTable("ActorMovie");
                });

            modelBuilder.Entity("StreamAppApi.Contracts.Models.Genre", b =>
                {
                    b.Property<string>("GenreId")
                        .HasColumnType("text")
                        .HasColumnName("_id");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("description");

                    b.Property<string>("Icon")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("icon");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");

                    b.Property<string>("Slug")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("slug");

                    b.HasKey("GenreId");

                    b.HasIndex("Slug")
                        .IsUnique();

                    b.ToTable("Genres", (string)null);
                });

            modelBuilder.Entity("StreamAppApi.Contracts.Models.GenreMovie", b =>
                {
                    b.Property<string>("GenreId")
                        .HasColumnType("text");

                    b.Property<string>("MovieId")
                        .HasColumnType("text");

                    b.HasKey("GenreId", "MovieId");

                    b.HasIndex("MovieId");

                    b.ToTable("GenreMovie");
                });

            modelBuilder.Entity("StreamAppApi.Contracts.Models.Movie", b =>
                {
                    b.Property<string>("MovieId")
                        .HasColumnType("text")
                        .HasColumnName("_id");

                    b.Property<string>("BigPoster")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("bigPoster");

                    b.Property<int?>("CountOpened")
                        .HasColumnType("integer")
                        .HasColumnName("countOpened");

                    b.Property<bool?>("IsSendTelegram")
                        .HasColumnType("boolean")
                        .HasColumnName("isSendTelegram");

                    b.Property<string>("Poster")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("poster");

                    b.Property<double?>("Rating")
                        .HasColumnType("double precision")
                        .HasColumnName("rating");

                    b.Property<string>("Slug")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("slug");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("title");

                    b.Property<string>("UserId")
                        .HasColumnType("text");

                    b.Property<string>("VideoUrl")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("videoUrl");

                    b.HasKey("MovieId");

                    b.HasIndex("Slug")
                        .IsUnique();

                    b.HasIndex("UserId");

                    b.ToTable("Movies", (string)null);
                });

            modelBuilder.Entity("StreamAppApi.Contracts.Models.MovieParameter", b =>
                {
                    b.Property<string>("ParameterId")
                        .HasColumnType("text")
                        .HasColumnName("_id");

                    b.Property<string>("Country")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("country");

                    b.Property<int>("Duration")
                        .HasColumnType("integer")
                        .HasColumnName("duration");

                    b.Property<int>("Year")
                        .HasColumnType("integer")
                        .HasColumnName("year");

                    b.HasKey("ParameterId");

                    b.ToTable("Parameters");
                });

            modelBuilder.Entity("StreamAppApi.Contracts.Models.User", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("text")
                        .HasColumnName("_id");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("email");

                    b.Property<bool>("IsAdmin")
                        .HasColumnType("boolean")
                        .HasColumnName("isAdmin");

                    b.Property<byte[]>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("bytea")
                        .HasColumnName("passwordHash");

                    b.Property<byte[]>("PasswordSalt")
                        .IsRequired()
                        .HasColumnType("bytea")
                        .HasColumnName("passwordSalt");

                    b.Property<string>("RefreshToken")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("refreshToken");

                    b.Property<DateTime>("TokenCreated")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("tokenCreated");

                    b.Property<DateTime>("TokenExpires")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("tokenUpdated");

                    b.HasKey("UserId");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.ToTable("Users", (string)null);
                });

            modelBuilder.Entity("StreamAppApi.Contracts.Models.UserMovie", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("text");

                    b.Property<string>("MovieId")
                        .HasColumnType("text");

                    b.HasKey("UserId", "MovieId");

                    b.HasIndex("MovieId");

                    b.ToTable("UserMovie");
                });

            modelBuilder.Entity("StreamAppApi.Contracts.Models.ActorMovie", b =>
                {
                    b.HasOne("StreamAppApi.Contracts.Models.Actor", "Actor")
                        .WithMany("Movies")
                        .HasForeignKey("ActorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("StreamAppApi.Contracts.Models.Movie", "Movie")
                        .WithMany("Actors")
                        .HasForeignKey("MovieId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Actor");

                    b.Navigation("Movie");
                });

            modelBuilder.Entity("StreamAppApi.Contracts.Models.GenreMovie", b =>
                {
                    b.HasOne("StreamAppApi.Contracts.Models.Genre", "Genre")
                        .WithMany("Movies")
                        .HasForeignKey("GenreId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("StreamAppApi.Contracts.Models.Movie", "Movie")
                        .WithMany("Genres")
                        .HasForeignKey("MovieId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Genre");

                    b.Navigation("Movie");
                });

            modelBuilder.Entity("StreamAppApi.Contracts.Models.Movie", b =>
                {
                    b.HasOne("StreamAppApi.Contracts.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId");

                    b.Navigation("User");
                });

            modelBuilder.Entity("StreamAppApi.Contracts.Models.MovieParameter", b =>
                {
                    b.HasOne("StreamAppApi.Contracts.Models.Movie", "Movie")
                        .WithOne("Parameters")
                        .HasForeignKey("StreamAppApi.Contracts.Models.MovieParameter", "ParameterId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Movie");
                });

            modelBuilder.Entity("StreamAppApi.Contracts.Models.UserMovie", b =>
                {
                    b.HasOne("StreamAppApi.Contracts.Models.Movie", "Movie")
                        .WithMany("Users")
                        .HasForeignKey("MovieId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("StreamAppApi.Contracts.Models.User", "User")
                        .WithMany("Favorites")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Movie");

                    b.Navigation("User");
                });

            modelBuilder.Entity("StreamAppApi.Contracts.Models.Actor", b =>
                {
                    b.Navigation("Movies");
                });

            modelBuilder.Entity("StreamAppApi.Contracts.Models.Genre", b =>
                {
                    b.Navigation("Movies");
                });

            modelBuilder.Entity("StreamAppApi.Contracts.Models.Movie", b =>
                {
                    b.Navigation("Actors");

                    b.Navigation("Genres");

                    b.Navigation("Parameters")
                        .IsRequired();

                    b.Navigation("Users");
                });

            modelBuilder.Entity("StreamAppApi.Contracts.Models.User", b =>
                {
                    b.Navigation("Favorites");
                });
#pragma warning restore 612, 618
        }
    }
}
