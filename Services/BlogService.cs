using SimpleBlog.Models.Entities;
using SimpleBlog.Models.ViewModels;
using SimpleBlog.Repositories.Interfaces;
using System.Text.RegularExpressions;

namespace SimpleBlog.Services
{
    /// <summary>
    /// Service for managing blog posts.
    /// Encapsulates business logic and handles slug generation.
    /// </summary>
    public class BlogService
    {
        private readonly IBlogPostRepository _postRepository;
        private readonly ICategoryRepository _categoryRepository;

        public BlogService(IBlogPostRepository postRepository, ICategoryRepository categoryRepository)
        {
            _postRepository = postRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<IEnumerable<BlogPost>> GetAllPostsAsync()
        {
            return await _postRepository.GetAllAsync();
        }

        public async Task<IEnumerable<BlogPost>> GetPublishedPostsAsync()
        {
            return await _postRepository.GetPublishedAsync();
        }

        public async Task<BlogPost?> GetPostByIdAsync(int id)
        {
            return await _postRepository.GetByIdAsync(id);
        }

        public async Task<BlogPost?> GetPostBySlugAsync(string slug)
        {
            return await _postRepository.GetBySlugAsync(slug);
        }

        public async Task<IEnumerable<BlogPost>> GetPostsByCategoryAsync(int categoryId)
        {
            return await _postRepository.GetByCategoryAsync(categoryId);
        }

        public async Task<IEnumerable<BlogPost>> GetPublishedPostsByCategoryAsync(int categoryId)
        {
            return await _postRepository.GetPublishedByCategoryAsync(categoryId);
        }

        public async Task<int> CreatePostAsync(BlogPostCreateViewModel model)
        {
            var slug = await GenerateUniqueSlugAsync(model.Title);

            var post = new BlogPost
            {
                Title = model.Title,
                Slug = slug,
                Content = model.Content,
                ShortDescription = model.ShortDescription,
                FeaturedImage = model.FeaturedImage,
                CategoryId = model.CategoryId,
                IsPublished = model.IsPublished,
                CreatedAt = DateTime.UtcNow
            };

            await _postRepository.AddAsync(post);
            return post.Id;
        }

        public async Task UpdatePostAsync(BlogPostEditViewModel model)
        {
            var post = await _postRepository.GetByIdAsync(model.Id);
            if (post == null)
                throw new InvalidOperationException("Post not found");

            post.Title = model.Title;
            post.Slug = await GenerateUniqueSlugAsync(model.Title, model.Id);
            post.Content = model.Content;
            post.ShortDescription = model.ShortDescription;
            post.FeaturedImage = model.FeaturedImage;
            post.CategoryId = model.CategoryId;
            post.IsPublished = model.IsPublished;
            post.UpdatedAt = DateTime.UtcNow;

            await _postRepository.UpdateAsync(post);
        }

        public async Task DeletePostAsync(int id)
        {
            await _postRepository.DeleteAsync(id);
        }

        private async Task<string> GenerateUniqueSlugAsync(string title, int? currentPostId = null)
        {
            var baseSlug = GenerateSlug(title);
            if (string.IsNullOrEmpty(baseSlug))
            {
                baseSlug = "post";
            }

            var slug = baseSlug;
            int counter = 1;

            while (true)
            {
                var existing = await _postRepository.GetBySlugAsync(slug);
                if (existing == null || (currentPostId.HasValue && existing.Id == currentPostId.Value))
                {
                    return slug;
                }

                slug = $"{baseSlug}-{counter}";
                counter++;
            }
        }

        /// <summary>
        /// Generate a URL-friendly slug from a title.
        /// Example: "My First Post" -> "my-first-post"
        /// </summary>
        private string GenerateSlug(string title)
        {
            var slug = title.ToLower()
                .Trim()
                .Replace(" ", "-");

            slug = Regex.Replace(slug, @"[^a-z0-9\-]", "");
            slug = Regex.Replace(slug, @"-+", "-");
            slug = slug.Trim('-');

            return slug;
        }
    }
}
