using System.ComponentModel.DataAnnotations;

namespace SimpleBlog.Models.Entities
{
    /// <summary>
    /// Represents an admin user.
    /// Kept simple for now but designed to support ASP.NET Identity migration later.
    /// </summary>
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
