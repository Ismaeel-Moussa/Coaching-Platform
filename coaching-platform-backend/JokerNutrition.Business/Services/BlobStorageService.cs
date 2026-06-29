using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using JokerNutrition.Business.Configurations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JokerNutrition.Business.Services;

public interface IBlobStorageService
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType);
}

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobStorageSettings _settings;
    private readonly IWebHostEnvironment _hostingEnv;
    private readonly ILogger<BlobStorageService> _logger;

    public BlobStorageService(
        IOptions<BlobStorageSettings> settings,
        IWebHostEnvironment hostingEnv,
        ILogger<BlobStorageService> logger)
    {
        _settings = settings.Value;
        _hostingEnv = hostingEnv;
        _logger = logger;
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
    {
        try
        {
            // Clean filename to avoid path issues
            var cleanFileName = $"{Guid.NewGuid()}_{Path.GetFileName(fileName)}";

            // If connection string is empty or default local without Azurite running, we can trigger the fallback directly.
            // But we can also try the standard SDK route:
            var blobServiceClient = new BlobServiceClient(_settings.ConnectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(_settings.ContainerName);
            
            // This will throw if local Azurite emulator is not running
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);
            
            var blobClient = containerClient.GetBlobClient(cleanFileName);
            
            if (fileStream.CanSeek)
            {
                fileStream.Position = 0;
            }

            await blobClient.UploadAsync(fileStream, new BlobHttpHeaders { ContentType = contentType });
            return blobClient.Uri.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Azure Blob Storage upload failed. Falling back to simulated local file storage.");
            return await SaveLocalFileAsync(fileStream, fileName);
        }
    }

    private async Task<string> SaveLocalFileAsync(Stream fileStream, string fileName)
    {
        var uploadsDir = Path.Combine(_hostingEnv.ContentRootPath, "wwwroot", "uploads");
        if (!Directory.Exists(uploadsDir))
        {
            Directory.CreateDirectory(uploadsDir);
        }

        var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(fileName)}";
        var filePath = Path.Combine(uploadsDir, uniqueFileName);

        using (var targetStream = File.Create(filePath))
        {
            if (fileStream.CanSeek)
            {
                fileStream.Position = 0;
            }
            await fileStream.CopyToAsync(targetStream);
        }

        // Return local hosting static file path
        return $"http://localhost:7000/uploads/{uniqueFileName}";
    }
}
