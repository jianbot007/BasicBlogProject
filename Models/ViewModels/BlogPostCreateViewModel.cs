using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace SimpleBlog.Models.ViewModels
{
    public class BlogPostCreateViewModel
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Short description is required")]
        [StringLength(500, ErrorMessage = "Short description cannot exceed 500 characters")]
        public string ShortDescription { get; set; } = string.Empty;

        [Required(ErrorMessage = "Content is required")]
        public string Content { get; set; } = string.Empty;

        [Display(Name = "Featured Image")]
        public IFormFile? FeaturedImageFile { get; set; }

        [StringLength(500, ErrorMessage = "Featured image URL cannot exceed 500 characters")]
        public string? FeaturedImage { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Category is required")]
        public int CategoryId { get; set; }

        public bool IsPublished { get; set; } = false;
    }
}
