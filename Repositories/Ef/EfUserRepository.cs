using Microsoft.EntityFrameworkCore;
using SimpleBlog.Data;
using SimpleBlog.Models.Entities;
using SimpleBlog.Repositories.Interfaces;
using SimpleBlog.Utilities;

namespace SimpleBlog.Repositories.Ef
{
    public class EfUserRepository : IUserRepository
    {
        private readonly BlogDbContext _db;

        public EfUserRepository(BlogDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(User user)
        {
            await _db.Users.AddAsync(user);
            await _db.SaveChangesAsync();
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _db.Users.FindAsync(id);
        }

        public async Task UpdateAsync(User user)
        {
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
        }

        public Task SaveAsync()
        {
            return Task.CompletedTask;
        }
    }
}
