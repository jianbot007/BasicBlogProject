using Microsoft.AspNetCore.Http;

namespace SimpleBlog.Services
{
    public interface IBlobStorageService
    {
        /// <summary>
        /// Uploads an image file to Azure Blob Storage and returns the full public blob URL.
        /// </summary>
        /// <param name="file">The uploaded image file</param>
        /// <returns>The resulting blob URL</returns>
        Task<string> UploadImageAsync(IFormFile file);
    }
}
