using System.Text.Json;
using SimpleBlog.Models.Entities;
using SimpleBlog.Repositories.Interfaces;

namespace SimpleBlog.Repositories.Json
{
    /// <summary>
    /// JSON-based implementation of ICategoryRepository.
    /// Can be replaced with EfCategoryRepository without changing consumers.
    /// </summary>
    public class JsonCategoryRepository : ICategoryRepository
    {
        private readonly string _filePath;
        private List<Category> _categories;

        public JsonCategoryRepository(IWebHostEnvironment environment)
        {
            _filePath = Path.Combine(environment.ContentRootPath, "Data", "categories.json");
            _categories = new List<Category>();
            LoadData();
        }

        public Task<IEnumerable<Category>> GetAllAsync()
        {
            return Task.FromResult(_categories.AsEnumerable());
        }

        public Task<Category?> GetByIdAsync(int id)
        {
            var category = _categories.FirstOrDefault(c => c.Id == id);
            return Task.FromResult(category);
        }

        public Task<Category?> GetBySlugAsync(string slug)
        {
            var category = _categories.FirstOrDefault(c => c.Slug == slug);
            return Task.FromResult(category);
        }

        public Task AddAsync(Category category)
        {
            category.Id = _categories.Any() ? _categories.Max(c => c.Id) + 1 : 1;
            _categories.Add(category);
            return SaveAsync();
        }

        public Task UpdateAsync(Category category)
        {
            var existing = _categories.FirstOrDefault(c => c.Id == category.Id);
            if (existing != null)
            {
                existing.Name = category.Name;
                existing.Slug = category.Slug;
                existing.Description = category.Description;
            }
            return SaveAsync();
        }

        public Task DeleteAsync(int id)
        {
            var category = _categories.FirstOrDefault(c => c.Id == id);
            if (category != null)
            {
                _categories.Remove(category);
            }
            return SaveAsync();
        }

        public Task SaveAsync()
        {
            var json = JsonSerializer.Serialize(_categories, new JsonSerializerOptions { WriteIndented = true });
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
                    _categories = JsonSerializer.Deserialize<List<Category>>(json) ?? new List<Category>();
                }
            }
            catch
            {
                _categories = new List<Category>();
            }
        }
    }
}
