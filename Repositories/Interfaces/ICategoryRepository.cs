using SimpleBlog.Models.Entities;

namespace SimpleBlog.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for Category entity.
    /// Designed to support multiple implementations (JSON, EF Core, etc.)
    /// </summary>
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetAllAsync();
        Task<Category?> GetByIdAsync(int id);
        Task<Category?> GetBySlugAsync(string slug);
        Task AddAsync(Category category);
        Task UpdateAsync(Category category);
        Task DeleteAsync(int id);
        Task SaveAsync();
    }
}
