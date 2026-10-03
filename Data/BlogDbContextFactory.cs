using Azure.Core;
using Azure.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SimpleBlog.Data
{
    public class BlogDbContextFactory : IDesignTimeDbContextFactory<BlogDbContext>
    {
        public BlogDbContext CreateDbContext(string[] args)
        {
            var connectionString =
                "Server=learndbwithblog.database.windows.net,1433;" +
                "Database=blogs_db;" +
                "Encrypt=True;" +
                "TrustServerCertificate=False;" +
                "Connection Timeout=30;";

            var credential = new DefaultAzureCredential();

            var token = credential.GetToken(
                new TokenRequestContext(
                    new[] { "https://database.windows.net/.default" }));

            var connection = new SqlConnection(connectionString)
            {
                AccessToken = token.Token
            };

            var optionsBuilder =
                new DbContextOptionsBuilder<BlogDbContext>();

            optionsBuilder.UseSqlServer(connection);

            return new BlogDbContext(optionsBuilder.Options);
        }
    }
}