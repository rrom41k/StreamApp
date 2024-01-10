using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NUlid;

namespace StreamAppApi.Contracts.Models;

public class User
{
    public User(string email, byte[] passwordHash, byte[] passwordSalt, bool isAdmin = false)
    {
        UserId = Convert.ToString(Ulid.NewUlid());
        Email = email;
        PasswordHash = passwordHash;
        PasswordSalt = passwordSalt;
        IsAdmin = isAdmin;
        Favorites = new HashSet<UserMovie>();
    }

    [Key] [Column("_id")] public string UserId { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress]
    [Column("email")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [Column("passwordHash")]
    public byte[] PasswordHash { get; set; }
    
    [Column("passwordSalt")]
    public byte[] PasswordSalt { get; set; }
    
    [Column("isAdmin")]
    public bool IsAdmin { get; set; }

    public ICollection<UserMovie> Favorites { get; set; }
    
    [Column("refreshToken")] public string RefreshToken { get; set; } = string.Empty;

    [Column("tokenCreated")] public DateTime TokenCreated { get; set; } = DateTime.UtcNow;
    
    [Column("tokenUpdated")] public DateTime TokenExpires { get; set; } = DateTime.UtcNow;
}