using Microsoft.EntityFrameworkCore;
using SimpleBlog.Models.Entities;

namespace SimpleBlog.Data
{
    public class BlogDbContext : DbContext
    {
        public BlogDbContext(DbContextOptions<BlogDbContext> options)
            : base(options)
        {
        }

        public DbSet<BlogPost> BlogPosts { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<BlogPost>()
                .HasOne(x => x.Category)
                .WithMany(x => x.BlogPosts)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BlogPost>()
                .HasIndex(x => x.Slug)
                .IsUnique();

            modelBuilder.Entity<Category>()
                .HasIndex(x => x.Slug)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(x => x.Email)
                .IsUnique();
        }
    }
}
