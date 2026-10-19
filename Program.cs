using Microsoft.EntityFrameworkCore;
using SimpleBlog.Repositories.Interfaces;
using SimpleBlog.Services;
using SimpleBlog.Data;
using SimpleBlog.Repositories.Ef;
using SimpleBlog.Utilities;
using SimpleBlog.Models.Entities;
using Azure.Identity;
using Microsoft.Data.SqlClient;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure EF Core with SQL Server
builder.Services.AddDbContext<BlogDbContext>(options =>
{
    var connectionString =
        builder.Configuration.GetConnectionString("DefaultConnection");

    var credential = new DefaultAzureCredential();

    var token = credential.GetToken(
        new Azure.Core.TokenRequestContext(
            new[] { "https://database.windows.net/.default" }));

    var connection = new SqlConnection(connectionString)
    {
        AccessToken = token.Token
    };

    options.UseSqlServer(connection, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null);
    });
});

// Register repositories - EF Core implementations
builder.Services.AddScoped<ICategoryRepository, EfCategoryRepository>();
builder.Services.AddScoped<IBlogPostRepository, EfBlogPostRepository>();
builder.Services.AddScoped<IUserRepository, EfUserRepository>();

// Register services
builder.Services.AddScoped<BlogService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();

// Add session for admin authentication
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Use session middleware
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "blog_post",
    pattern: "Blog/Post/{slug}",
    defaults: new { controller = "Blog", action = "Post" });

app.MapControllerRoute(
    name: "blog_category",
    pattern: "Blog/Category/{slug}",
    defaults: new { controller = "Blog", action = "Category" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Seed initial data (admin user and default category) on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var db = services.GetRequiredService<BlogDbContext>();

        // Seed default category if none exists
        if (!db.Categories.Any())
        {
            var cat = new Category
            {
                Name = "Uncategorized",
                Slug = "uncategorized",
                Description = "Default category"
            };
            db.Categories.Add(cat);
            db.SaveChanges();
        }

        // Seed admin user if none exists with demo credentials
        var adminUser = db.Users.FirstOrDefault(u => u.Email == "admin@simpleblog.com");
        if (adminUser == null)
        {
            var admin = new User
            {
                Name = "Administrator",
                Email = "admin@simpleblog.com",
                PasswordHash = PasswordHelper.HashPassword("Admin@1234"),
                CreatedAt = DateTime.UtcNow
            };
            db.Users.Add(admin);
            db.SaveChanges();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error while seeding database: {ex.Message}");
    }
}

app.Run();
