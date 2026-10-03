using Microsoft.AspNetCore.Mvc;
using SimpleBlog.Services;

namespace SimpleBlog.Controllers
{
    /// <summary>
    /// Controller for public blog homepage and post browsing.
    /// </summary>
    public class BlogController : Controller
    {
        private readonly BlogService _blogService;
        private readonly CategoryService _categoryService;

        public BlogController(BlogService blogService, CategoryService categoryService)
        {
            _blogService = blogService;
            _categoryService = categoryService;
        }

        // GET: /Blog/Index - List all published posts
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var posts = await _blogService.GetPublishedPostsAsync();
            var categories = await _categoryService.GetAllCategoriesAsync();

            ViewBag.Categories = categories;
            return View(posts);
        }

        // GET: /Blog/Post/my-post-slug - View single post by slug
        [HttpGet("Blog/Post/{slug?}")]
        public async Task<IActionResult> Post(string? slug, string? id)
        {
            slug = !string.IsNullOrEmpty(slug) ? slug : id;
            if (string.IsNullOrEmpty(slug))
                return NotFound();

            var post = await _blogService.GetPostBySlugAsync(slug);
            if (post == null || !post.IsPublished)
                return NotFound();

            var categories = await _categoryService.GetAllCategoriesAsync();
            ViewBag.Categories = categories;
            return View(post);
        }

        // GET: /Blog/Category/technology - View posts by category
        [HttpGet("Blog/Category/{slug?}")]
        public async Task<IActionResult> Category(string? slug, string? id)
        {
            slug = !string.IsNullOrEmpty(slug) ? slug : id;
            if (string.IsNullOrEmpty(slug))
                return NotFound();

            var category = await _categoryService.GetCategoryBySlugAsync(slug);
            if (category == null)
                return NotFound();

            var posts = await _blogService.GetPublishedPostsByCategoryAsync(category.Id);
            var categories = await _categoryService.GetAllCategoriesAsync();

            ViewBag.CurrentCategory = category;
            ViewBag.Categories = categories;
            return View(posts);
        }
    }
}
