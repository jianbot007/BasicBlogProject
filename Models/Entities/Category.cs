using System.ComponentModel.DataAnnotations;

namespace SimpleBlog.Models.Entities
{
    /// <summary>
    /// Represents a blog post category.
    /// Designed for future EF Core migration with minimal changes.
    /// </summary>
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Slug { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        // Navigation property for EF Core compatibility
        public ICollection<BlogPost> BlogPosts { get; set; } = new List<BlogPost>();
    }
}
