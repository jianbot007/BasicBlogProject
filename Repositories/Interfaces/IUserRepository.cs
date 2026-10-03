using SimpleBlog.Models.Entities;

namespace SimpleBlog.Repositories.Interfaces
{
    /// <summary>
    /// Repository interface for User entity.
    /// Designed to support multiple implementations (JSON, ASP.NET Identity, etc.)
    /// </summary>
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByEmailAsync(string email);
        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task SaveAsync();
    }
}
