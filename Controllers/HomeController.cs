using System.Diagnostics;
using BasicProject.Models;
using Microsoft.AspNetCore.Mvc;
using SimpleBlog.Services;

namespace BasicProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly BlogService _blogService;
        private readonly CategoryService _categoryService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(BlogService blogService, CategoryService categoryService, ILogger<HomeController> logger)
        {
            _blogService = blogService;
            _categoryService = categoryService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            // Get latest published posts
            var posts = (await _blogService.GetPublishedPostsAsync()).Take(6);
            var categories = await _categoryService.GetAllCategoriesAsync();

            ViewBag.Categories = categories;
            return View(posts);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
