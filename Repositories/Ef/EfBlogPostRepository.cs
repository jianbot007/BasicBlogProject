using Microsoft.EntityFrameworkCore;
using SimpleBlog.Data;
using SimpleBlog.Models.Entities;
using SimpleBlog.Repositories.Interfaces;

namespace SimpleBlog.Repositories.Ef
{
    public class EfBlogPostRepository : IBlogPostRepository
    {
        private readonly BlogDbContext _db;

        public EfBlogPostRepository(BlogDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(BlogPost post)
        {
            await _db.BlogPosts.AddAsync(post);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var existing = await _db.BlogPosts.FindAsync(id);
            if (existing != null)
            {
                _db.BlogPosts.Remove(existing);
                await _db.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<BlogPost>> GetAllAsync()
        {
            return await _db.BlogPosts.Include(p => p.Category).AsNoTracking().ToListAsync();
        }

        public async Task<BlogPost?> GetByIdAsync(int id)
        {
            return await _db.BlogPosts.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<BlogPost?> GetBySlugAsync(string slug)
        {
            return await _db.BlogPosts.Include(p => p.Category).FirstOrDefaultAsync(p => p.Slug == slug);
        }

        public async Task<IEnumerable<BlogPost>> GetByCategoryAsync(int categoryId)
        {
            return await _db.BlogPosts.Include(p => p.Category)
                .Where(p => p.CategoryId == categoryId)
                .OrderByDescending(p => p.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<BlogPost>> GetPublishedAsync()
        {
            return await _db.BlogPosts.Include(p => p.Category)
                .Where(p => p.IsPublished)
                .OrderByDescending(p => p.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<BlogPost>> GetPublishedByCategoryAsync(int categoryId)
        {
            return await _db.BlogPosts.Include(p => p.Category)
                .Where(p => p.CategoryId == categoryId && p.IsPublished)
                .OrderByDescending(p => p.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task UpdateAsync(BlogPost post)
        {
            _db.BlogPosts.Update(post);
            await _db.SaveChangesAsync();
        }

        public Task SaveAsync()
        {
            return Task.CompletedTask;
        }
    }
}
