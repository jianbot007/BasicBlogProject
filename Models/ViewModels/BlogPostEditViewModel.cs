using System.ComponentModel.DataAnnotations;

namespace SimpleBlog.Models.ViewModels
{
    public class BlogPostEditViewModel : BlogPostCreateViewModel
    {
        public int Id { get; set; }
    }
}
