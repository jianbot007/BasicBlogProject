using Microsoft.EntityFrameworkCore;
using SimpleBlog.Data;
using SimpleBlog.Models.Entities;
using SimpleBlog.Repositories.Interfaces;

namespace SimpleBlog.Repositories.Ef
{
    public class EfCategoryRepository : ICategoryRepository
    {
        private readonly BlogDbContext _db;

        public EfCategoryRepository(BlogDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(Category category)
        {
            await _db.Categories.AddAsync(category);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var existing = await _db.Categories.FindAsync(id);
            if (existing != null)
            {
                _db.Categories.Remove(existing);
                await _db.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _db.Categories.AsNoTracking().ToListAsync();
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _db.Categories.FindAsync(id);
        }

        public async Task<Category?> GetBySlugAsync(string slug)
        {
            return await _db.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Slug == slug);
        }

        public async Task UpdateAsync(Category category)
        {
            _db.Categories.Update(category);
            await _db.SaveChangesAsync();
        }

        public Task SaveAsync()
        {
            // Operations already save changes. Provide no-op to satisfy interface.
            return Task.CompletedTask;
        }
    }
}
