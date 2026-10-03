using System.Text.Json;
using System.Security.Cryptography;
using SimpleBlog.Models.Entities;
using SimpleBlog.Repositories.Interfaces;

namespace SimpleBlog.Repositories.Json
{
    /// <summary>
    /// JSON-based implementation of IUserRepository.
    /// Can be replaced with ASP.NET Identity or custom database implementation.
    /// </summary>
    public class JsonUserRepository : IUserRepository
    {
        private readonly string _filePath;
        private List<User> _users;

        public JsonUserRepository(IWebHostEnvironment environment)
        {
            _filePath = Path.Combine(environment.ContentRootPath, "Data", "users.json");
            _users = new List<User>();
            LoadData();
        }

        public Task<User?> GetByIdAsync(int id)
        {
            var user = _users.FirstOrDefault(u => u.Id == id);
            return Task.FromResult(user);
        }

        public Task<User?> GetByEmailAsync(string email)
        {
            var user = _users.FirstOrDefault(u => u.Email == email);
            return Task.FromResult(user);
        }

        public Task AddAsync(User user)
        {
            user.Id = _users.Any() ? _users.Max(u => u.Id) + 1 : 1;
            _users.Add(user);
            return SaveAsync();
        }

        public Task UpdateAsync(User user)
        {
            var existing = _users.FirstOrDefault(u => u.Id == user.Id);
            if (existing != null)
            {
                existing.Name = user.Name;
                existing.Email = user.Email;
                existing.PasswordHash = user.PasswordHash;
            }
            return SaveAsync();
        }

        public Task SaveAsync()
        {
            var json = JsonSerializer.Serialize(_users, new JsonSerializerOptions { WriteIndented = true });
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
                    _users = JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
                }
            }
            catch
            {
                _users = new List<User>();
            }
        }

        /// <summary>
        /// Simple password hashing utility. In production, use bcrypt or similar.
        /// </summary>
        public static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        /// <summary>
        /// Verify password against hash.
        /// </summary>
        public static bool VerifyPassword(string password, string hash)
        {
            var hashOfInput = HashPassword(password);
            return hashOfInput == hash;
        }
    }
}
