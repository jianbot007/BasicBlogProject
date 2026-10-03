using Microsoft.AspNetCore.Mvc;
using SimpleBlog.Models.ViewModels;
using SimpleBlog.Services;
using SimpleBlog.Repositories.Interfaces;
using SimpleBlog.Utilities;

namespace SimpleBlog.Controllers
{
    /// <summary>
    /// Controller for admin area - authentication and dashboard.
    /// </summary>
    [Route("admin")]
    public class AdminController : Controller
    {
        private readonly BlogService _blogService;
        private readonly CategoryService _categoryService;
        private readonly IUserRepository _userRepository;
        private readonly IBlogPostRepository _postRepository;

        private const string AdminSessionKey = "AdminUserId";

        public AdminController(
            BlogService blogService,
            CategoryService categoryService,
            IUserRepository userRepository,
            IBlogPostRepository postRepository)
        {
            _blogService = blogService;
            _categoryService = categoryService;
            _userRepository = userRepository;
            _postRepository = postRepository;
        }

        // GET: /admin/login
        [HttpGet("login")]
        public IActionResult Login()
        {
            if (IsAdminLoggedIn())
                return RedirectToAction("Dashboard");

            return View();
        }

        // POST: /admin/login
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userRepository.GetByEmailAsync(model.Email);
            if (user == null || !PasswordHelper.VerifyPassword(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Invalid email or password");
                return View(model);
            }

            // Set session
            HttpContext.Session.SetInt32(AdminSessionKey, user.Id);
            return RedirectToAction("Dashboard");
        }

        // GET: /admin/logout
        [HttpGet("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove(AdminSessionKey);
            return RedirectToAction("Index", "Home");
        }

        // GET: /admin/dashboard
        [HttpGet("dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            var allPosts = await _blogService.GetAllPostsAsync();
            var publishedPosts = await _blogService.GetPublishedPostsAsync();
            var categories = await _categoryService.GetAllCategoriesAsync();

            var stats = new
            {
                TotalPosts = allPosts.Count(),
                PublishedPosts = publishedPosts.Count(),
                DraftPosts = allPosts.Count() - publishedPosts.Count(),
                TotalCategories = categories.Count()
            };

            ViewBag.Stats = stats;
            return View();
        }

        // GET: /admin/posts
        [HttpGet("posts")]
        public async Task<IActionResult> Posts()
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            var posts = await _blogService.GetAllPostsAsync();
            return View(posts);
        }

        // GET: /admin/posts/create
        [HttpGet("posts/create")]
        public async Task<IActionResult> CreatePost()
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            var categories = await _categoryService.GetAllCategoriesAsync();
            ViewBag.Categories = categories;
            return View();
        }

        // POST: /admin/posts/create
        [HttpPost("posts/create")]
        public async Task<IActionResult> CreatePost(BlogPostCreateViewModel model)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            if (!ModelState.IsValid)
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                ViewBag.Categories = categories;
                return View(model);
            }

            var postId = await _blogService.CreatePostAsync(model);
            return RedirectToAction("EditPost", new { id = postId });
        }

        // GET: /admin/posts/edit/5
        [HttpGet("posts/edit/{id}")]
        public async Task<IActionResult> EditPost(int id)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            var post = await _blogService.GetPostByIdAsync(id);
            if (post == null)
                return NotFound();

            var model = new BlogPostEditViewModel
            {
                Id = post.Id,
                Title = post.Title,
                ShortDescription = post.ShortDescription,
                Content = post.Content,
                FeaturedImage = post.FeaturedImage,
                CategoryId = post.CategoryId,
                IsPublished = post.IsPublished
            };

            var categories = await _categoryService.GetAllCategoriesAsync();
            ViewBag.Categories = categories;
            return View(model);
        }

        // POST: /admin/posts/edit/5
        [HttpPost("posts/edit/{id}")]
        public async Task<IActionResult> EditPost(int id, BlogPostEditViewModel model)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            if (id != model.Id)
                return BadRequest();

            if (!ModelState.IsValid)
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                ViewBag.Categories = categories;
                return View(model);
            }

            try
            {
                await _blogService.UpdatePostAsync(model);
                return RedirectToAction("Posts");
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
        }

        // POST: /admin/posts/delete/5
        [HttpPost("posts/delete/{id}")]
        public async Task<IActionResult> DeletePost(int id)
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            await _blogService.DeletePostAsync(id);
            return RedirectToAction("Posts");
        }

        // GET: /admin/categories
        [HttpGet("categories")]
        public async Task<IActionResult> Categories()
        {
            if (!IsAdminLoggedIn())
                return RedirectToAction("Login");

            var categories = await _categoryService.GetAllCategoriesAsync();
            return View(categories);
        }

        private bool IsAdminLoggedIn()
        {
            return HttpContext.Session.GetInt32(AdminSessionKey) != null;
        }
    }
}
