using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimpleBlog.Models.Entities
{
    /// <summary>
    /// Represents a blog post.
    /// Designed for future EF Core migration with minimal changes.
    /// </summary>
    public class BlogPost
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Slug { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string ShortDescription { get; set; } = string.Empty;

        [StringLength(500)]
        public string? FeaturedImage { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public bool IsPublished { get; set; } = false;

        [ForeignKey("Category")]
        public int CategoryId { get; set; }

        // Navigation property for EF Core compatibility
        public Category? Category { get; set; }
    }
}
