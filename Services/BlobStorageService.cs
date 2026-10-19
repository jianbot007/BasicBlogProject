using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SimpleBlog.Services
{
    public class BlobStorageService : IBlobStorageService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<BlobStorageService> _logger;
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long MaxFileSizeInBytes = 5 * 1024 * 1024; // 5 MB

        public BlobStorageService(IConfiguration configuration, ILogger<BlobStorageService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("No image file was provided for upload.", nameof(file));
            }

            if (file.Length > MaxFileSizeInBytes)
            {
                throw new ArgumentException("Image file size exceeds the allowed limit of 5 MB.", nameof(file));
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
            {
                throw new ArgumentException($"Invalid file extension '{extension}'. Only .jpg, .jpeg, .png, and .webp are allowed.", nameof(file));
            }

            var blobServiceClient = GetBlobServiceClient();
            var containerName = _configuration["BlobStorage:ContainerName"] ?? "blog-images";
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            // Generate a unique blob name to avoid accidental overwrites
            var uniqueBlobName = $"{Guid.NewGuid():N}{extension}";
            var blobClient = containerClient.GetBlobClient(uniqueBlobName);

            var uploadOptions = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = string.IsNullOrWhiteSpace(file.ContentType) ? GetContentType(extension) : file.ContentType
                }
            };

            await using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, uploadOptions);

            _logger.LogInformation("Image successfully uploaded to Azure Blob Storage: {BlobUri}", blobClient.Uri);
            return blobClient.Uri.ToString();
        }

        private BlobServiceClient GetBlobServiceClient()
        {
            var connectionString = _configuration.GetConnectionString("BlobStorage")
                                  ?? _configuration["BlobStorage:ConnectionString"];

            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                return new BlobServiceClient(connectionString);
            }

            var accountName = _configuration["BlobStorage:AccountName"];

            if (!string.IsNullOrWhiteSpace(accountName))
            {
                var serviceUri = new Uri(
                    $"https://{accountName}.blob.core.windows.net");

                return new BlobServiceClient(
                    serviceUri,
                    new DefaultAzureCredential());
            }

            throw new InvalidOperationException(
                "Azure Blob Storage is not configured. Please provide 'BlobStorage:ConnectionString' " +
                "or 'BlobStorage:ServiceUri' in your configuration (appsettings.Development.json, User Secrets, " +
                "or Azure App Service Application Settings).");
        }

        private static string GetContentType(string extension) => extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };
    }
}
