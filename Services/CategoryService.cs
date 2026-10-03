using SimpleBlog.Models.Entities;
using SimpleBlog.Repositories.Interfaces;
using System.Text.RegularExpressions;

namespace SimpleBlog.Services
{
    /// <summary>
    /// Service for managing categories.
    /// Encapsulates business logic and handles slug generation.
    /// </summary>
    public class CategoryService
    {
        private readonly ICategoryRepository _repository;

        public CategoryService(ICategoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<Category?> GetCategoryBySlugAsync(string slug)
        {
            return await _repository.GetBySlugAsync(slug);
        }

        public async Task<int> CreateCategoryAsync(string name, string? description = null)
        {
            var slug = GenerateSlug(name);

            var category = new Category
            {
                Name = name,
                Slug = slug,
                Description = description
            };

            await _repository.AddAsync(category);
            return category.Id;
        }

        public async Task UpdateCategoryAsync(int id, string name, string? description = null)
        {
            var category = await _repository.GetByIdAsync(id);
            if (category == null)
                throw new InvalidOperationException("Category not found");

            category.Name = name;
            category.Slug = GenerateSlug(name);
            category.Description = description;

            await _repository.UpdateAsync(category);
        }

        public async Task DeleteCategoryAsync(int id)
        {
            await _repository.DeleteAsync(id);
        }

        /// <summary>
        /// Generate a URL-friendly slug from a category name.
        /// </summary>
        private string GenerateSlug(string name)
        {
            var slug = name.ToLower()
                .Trim()
                .Replace(" ", "-");

            slug = Regex.Replace(slug, @"[^a-z0-9\-]", "");
            slug = Regex.Replace(slug, @"-+", "-");
            slug = slug.Trim('-');

            return slug;
        }
    }
}
