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
        private readonly IBlobStorageService _blobStorageService;
        private readonly ILogger<AdminController> _logger;

        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long MaxFileSizeInBytes = 5 * 1024 * 1024; // 5 MB

        private const string AdminSessionKey = "AdminUserId";

        public AdminController(
            BlogService blogService,
            CategoryService categoryService,
            IUserRepository userRepository,
            IBlogPostRepository postRepository,
            IBlobStorageService blobStorageService,
            ILogger<AdminController> logger)
        {
            _blogService = blogService;
            _categoryService = categoryService;
            _userRepository = userRepository;
            _postRepository = postRepository;
            _blobStorageService = blobStorageService;
            _logger = logger;
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

            // Validate that an image file is provided
            if (model.FeaturedImageFile == null || model.FeaturedImageFile.Length == 0)
            {
                ModelState.AddModelError(nameof(model.FeaturedImageFile), "Please select a featured image file.");
            }
            else if (!ValidateImageFile(model.FeaturedImageFile, out var validationError))
            {
                ModelState.AddModelError(nameof(model.FeaturedImageFile), validationError!);
            }

            if (!ModelState.IsValid)
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                ViewBag.Categories = categories;
                return View(model);
            }

            // Upload image to Azure Blob Storage
            try
            {
                var imageUrl = await _blobStorageService.UploadImageAsync(model.FeaturedImageFile!);
                model.FeaturedImage = imageUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload image to Azure Blob Storage during post creation.");
                ModelState.AddModelError(nameof(model.FeaturedImageFile), $"Storage upload failed: {ex.Message}");
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

            // If a replacement image file was uploaded, validate it
            if (model.FeaturedImageFile != null && model.FeaturedImageFile.Length > 0)
            {
                if (!ValidateImageFile(model.FeaturedImageFile, out var validationError))
                {
                    ModelState.AddModelError(nameof(model.FeaturedImageFile), validationError!);
                }
            }

            if (!ModelState.IsValid)
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                ViewBag.Categories = categories;
                return View(model);
            }

            // If a new image was uploaded, upload to Azure Blob Storage to replace current image URL
            if (model.FeaturedImageFile != null && model.FeaturedImageFile.Length > 0)
            {
                try
                {
                    var imageUrl = await _blobStorageService.UploadImageAsync(model.FeaturedImageFile);
                    model.FeaturedImage = imageUrl;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to upload replacement image to Azure Blob Storage for post {PostId}.", id);
                    ModelState.AddModelError(nameof(model.FeaturedImageFile), $"Storage upload failed: {ex.Message}");
                    var categories = await _categoryService.GetAllCategoriesAsync();
                    ViewBag.Categories = categories;
                    return View(model);
                }
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

        private static bool ValidateImageFile(IFormFile file, out string? errorMessage)
        {
            errorMessage = null;

            if (file == null || file.Length == 0)
            {
                errorMessage = "Please select an image file.";
                return false;
            }

            if (file.Length > MaxFileSizeInBytes)
            {
                errorMessage = "Image size cannot exceed 5 MB.";
                return false;
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                errorMessage = "Only .jpg, .jpeg, .png, and .webp image files are allowed.";
                return false;
            }

            return true;
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
