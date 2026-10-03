using System.Text.Json;
using SimpleBlog.Models.Entities;
using SimpleBlog.Repositories.Interfaces;

namespace SimpleBlog.Repositories.Json
{
    /// <summary>
    /// JSON-based implementation of IBlogPostRepository.
    /// Can be replaced with EfBlogPostRepository without changing consumers.
    /// </summary>
    public class JsonBlogPostRepository : IBlogPostRepository
    {
        private readonly string _filePath;
        private List<BlogPost> _posts;
        private readonly ICategoryRepository _categoryRepository;

        public JsonBlogPostRepository(IWebHostEnvironment environment, ICategoryRepository categoryRepository)
        {
            _filePath = Path.Combine(environment.ContentRootPath, "Data", "posts.json");
            _posts = new List<BlogPost>();
            _categoryRepository = categoryRepository;
            LoadData();
        }

        public async Task<IEnumerable<BlogPost>> GetAllAsync()
        {
            await LoadCategoryReferences();
            return _posts.AsEnumerable();
        }

        public async Task<BlogPost?> GetByIdAsync(int id)
        {
            var post = _posts.FirstOrDefault(p => p.Id == id);
            if (post != null)
            {
                post.Category ??= await _categoryRepository.GetByIdAsync(post.CategoryId);
            }
            return post;
        }

        public async Task<BlogPost?> GetBySlugAsync(string slug)
        {
            var post = _posts.FirstOrDefault(p => p.Slug == slug);
            if (post != null)
            {
                post.Category ??= await _categoryRepository.GetByIdAsync(post.CategoryId);
            }
            return post;
        }

        public async Task<IEnumerable<BlogPost>> GetPublishedAsync()
        {
            await LoadCategoryReferences();
            return _posts.Where(p => p.IsPublished).OrderByDescending(p => p.CreatedAt).AsEnumerable();
        }

        public async Task<IEnumerable<BlogPost>> GetByCategoryAsync(int categoryId)
        {
            await LoadCategoryReferences();
            return _posts.Where(p => p.CategoryId == categoryId).OrderByDescending(p => p.CreatedAt).AsEnumerable();
        }

        public async Task<IEnumerable<BlogPost>> GetPublishedByCategoryAsync(int categoryId)
        {
            await LoadCategoryReferences();
            return _posts.Where(p => p.CategoryId == categoryId && p.IsPublished).OrderByDescending(p => p.CreatedAt).AsEnumerable();
        }

        public Task AddAsync(BlogPost post)
        {
            post.Id = _posts.Any() ? _posts.Max(p => p.Id) + 1 : 1;
            post.CreatedAt = DateTime.UtcNow;
            _posts.Add(post);
            return SaveAsync();
        }

        public Task UpdateAsync(BlogPost post)
        {
            var existing = _posts.FirstOrDefault(p => p.Id == post.Id);
            if (existing != null)
            {
                existing.Title = post.Title;
                existing.Slug = post.Slug;
                existing.Content = post.Content;
                existing.ShortDescription = post.ShortDescription;
                existing.FeaturedImage = post.FeaturedImage;
                existing.CategoryId = post.CategoryId;
                existing.IsPublished = post.IsPublished;
                existing.UpdatedAt = DateTime.UtcNow;
            }
            return SaveAsync();
        }

        public Task DeleteAsync(int id)
        {
            var post = _posts.FirstOrDefault(p => p.Id == id);
            if (post != null)
            {
                _posts.Remove(post);
            }
            return SaveAsync();
        }

        public Task SaveAsync()
        {
            var json = JsonSerializer.Serialize(_posts, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
            return Task.CompletedTask;
        }

        private void LoadData()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    var json = File.ReadAllText(_filePath);
                    _posts = JsonSerializer.Deserialize<List<BlogPost>>(json) ?? new List<BlogPost>();
                }
            }
            catch
            {
                _posts = new List<BlogPost>();
            }
        }

        private async Task LoadCategoryReferences()
        {
            foreach (var post in _posts.Where(p => p.Category == null))
            {
                post.Category = await _categoryRepository.GetByIdAsync(post.CategoryId);
            }
        }
    }
}
