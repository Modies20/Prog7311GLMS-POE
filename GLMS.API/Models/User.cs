using System.ComponentModel.DataAnnotations;

namespace GLMS.API.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public string Role { get; set; } = "User"; // Admin, User, Manager
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}