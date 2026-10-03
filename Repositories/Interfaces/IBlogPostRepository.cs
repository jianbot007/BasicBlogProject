using SimpleBlog.Models.Entities;

namespace SimpleBlog.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for BlogPost entity.
    /// Designed to support multiple implementations (JSON, EF Core, etc.)
    /// </summary>
    public interface IBlogPostRepository
    {
        Task<IEnumerable<BlogPost>> GetAllAsync();
        Task<BlogPost?> GetByIdAsync(int id);
        Task<BlogPost?> GetBySlugAsync(string slug);
        Task<IEnumerable<BlogPost>> GetPublishedAsync();
        Task<IEnumerable<BlogPost>> GetByCategoryAsync(int categoryId);
        Task<IEnumerable<BlogPost>> GetPublishedByCategoryAsync(int categoryId);
        Task AddAsync(BlogPost post);
        Task UpdateAsync(BlogPost post);
        Task DeleteAsync(int id);
        Task SaveAsync();
    }
}
