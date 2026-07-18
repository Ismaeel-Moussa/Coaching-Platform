using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using JokerNutrition.Business.Configurations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JokerNutrition.Business.Services;

public interface IBlobStorageService
{
    Task<string> UploadPublicAssetAsync(Stream fileStream, string fileName, string contentType);
    Task<string> UploadPrivatePhotoAsync(Stream fileStream, string fileName, string contentType);
    Task<string> GetReadUrlAsync(string fileUrl, TimeSpan? lifetime = null);
    Task<byte[]?> DownloadPrivateImageAsync(
        string fileUrl,
        int maximumBytes,
        CancellationToken cancellationToken = default);
    Task DeleteFileAsync(string fileUrl);
}

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobStorageSettings _settings;
    private readonly IWebHostEnvironment _hostingEnv;
    private readonly ILogger<BlobStorageService> _logger;
    private readonly Lazy<BlobServiceClient> _blobServiceClient;

    public BlobStorageService(
        IOptions<BlobStorageSettings> settings,
        IWebHostEnvironment hostingEnv,
        ILogger<BlobStorageService> logger)
    {
        _settings = settings.Value;
        _hostingEnv = hostingEnv;
        _logger = logger;
        _blobServiceClient = new Lazy<BlobServiceClient>(
            () => new BlobServiceClient(_settings.ConnectionString),
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public Task<string> UploadPublicAssetAsync(Stream fileStream, string fileName, string contentType) =>
        UploadFileAsync(fileStream, fileName, contentType, isPrivate: false);

    public Task<string> UploadPrivatePhotoAsync(Stream fileStream, string fileName, string contentType) =>
        UploadFileAsync(fileStream, fileName, contentType, isPrivate: true);

    private async Task<string> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        bool isPrivate = false)
    {
        try
        {
            var normalizedContentType = contentType.Equals("image/jpg", StringComparison.OrdinalIgnoreCase)
                ? "image/jpeg"
                : contentType;
            // Clean filename to avoid path issues
            var cleanFileName = $"{Guid.NewGuid()}_{Path.GetFileName(fileName)}";

            // If connection string is empty or default local without Azurite running, we can trigger the fallback directly.
            // But we can also try the standard SDK route:
            var blobServiceClient = _blobServiceClient.Value;
            var containerName = isPrivate ? _settings.PrivateContainerName : _settings.ContainerName;
            if (isPrivate && (string.IsNullOrWhiteSpace(_settings.PrivateContainerName) ||
                              string.IsNullOrWhiteSpace(_settings.ContainerName) ||
                              _settings.PrivateContainerName.Equals(_settings.ContainerName, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("Public and private blob container names must be configured and different.");
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            
            // This will throw if local Azurite emulator is not running
            await containerClient.CreateIfNotExistsAsync(
                isPrivate ? PublicAccessType.None : PublicAccessType.Blob);
            if (isPrivate)
            {
                var properties = await containerClient.GetPropertiesAsync();
                if (properties.Value.PublicAccess != PublicAccessType.None)
                    throw new InvalidOperationException("The private photo container permits anonymous access.");
            }
            
            var blobClient = containerClient.GetBlobClient(cleanFileName);
            
            if (fileStream.CanSeek)
            {
                fileStream.Position = 0;
            }

            await blobClient.UploadAsync(fileStream, new BlobHttpHeaders
            {
                ContentType = normalizedContentType,
                CacheControl = isPrivate ? "private, no-store" : "public, max-age=86400"
            });
            return blobClient.Uri.ToString();
        }
        catch (Exception ex)
        {
            if (isPrivate)
            {
                _logger.LogError(ex, "Private Azure Blob Storage upload failed; public fallback is disabled.");
                throw new InvalidOperationException("Private photo storage is temporarily unavailable.", ex);
            }

            _logger.LogWarning(ex, "Azure Blob Storage upload failed. Falling back to simulated local file storage.");
            return await SaveLocalFileAsync(fileStream, fileName);
        }
    }

    public Task<string> GetReadUrlAsync(string fileUrl, TimeSpan? lifetime = null)
    {
        if (string.IsNullOrWhiteSpace(fileUrl) || fileUrl.Contains("/uploads/", StringComparison.OrdinalIgnoreCase))
            return Task.FromResult(fileUrl);

        try
        {
            if (!Uri.TryCreate(fileUrl, UriKind.Absolute, out var uri))
                return Task.FromResult(fileUrl);

            var source = new BlobUriBuilder(uri);
            if (string.IsNullOrWhiteSpace(source.BlobContainerName) || string.IsNullOrWhiteSpace(source.BlobName))
                return Task.FromResult(fileUrl);

            var serviceClient = _blobServiceClient.Value;
            if (!string.Equals(uri.Host, serviceClient.Uri.Host, StringComparison.OrdinalIgnoreCase) ||
                !IsAllowedContainer(source.BlobContainerName))
            {
                _logger.LogWarning("Refusing to sign a blob URL from an unexpected host: {BlobHost}", uri.Host);
                return Task.FromResult(fileUrl);
            }

            var blobClient = serviceClient
                .GetBlobContainerClient(source.BlobContainerName)
                .GetBlobClient(source.BlobName);
            if (!blobClient.CanGenerateSasUri)
            {
                _logger.LogWarning("Blob client cannot generate a read SAS for {BlobUrl}.", uri.GetLeftPart(UriPartial.Path));
                if (source.BlobContainerName.Equals(_settings.PrivateContainerName, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("Private photo access is temporarily unavailable.");
                return Task.FromResult(fileUrl);
            }

            var sas = new BlobSasBuilder
            {
                BlobContainerName = source.BlobContainerName,
                BlobName = source.BlobName,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.Add(lifetime ?? TimeSpan.FromMinutes(15)),
                CacheControl = "private, no-store"
            };
            sas.SetPermissions(BlobSasPermissions.Read);
            var signedUri = blobClient.GenerateSasUri(sas);
            return Task.FromResult(signedUri.ToString());
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Could not generate a signed read URL for a stored image.");
            if (IsPrivateBlobUrl(fileUrl))
                throw new InvalidOperationException("Private photo access is temporarily unavailable.", exception);
            return Task.FromResult(fileUrl);
        }
    }

    public async Task<byte[]?> DownloadPrivateImageAsync(
        string fileUrl,
        int maximumBytes,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fileUrl) || maximumBytes <= 0)
            return null;

        if (fileUrl.Contains("/uploads/", StringComparison.OrdinalIgnoreCase))
        {
            if (!Uri.TryCreate(fileUrl, UriKind.Absolute, out var localUri) ||
                !Uri.TryCreate(_settings.LocalFallbackBaseUrl, UriKind.Absolute, out var fallbackUri) ||
                !localUri.AbsolutePath.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase) ||
                !localUri.Scheme.Equals(fallbackUri.Scheme, StringComparison.OrdinalIgnoreCase) ||
                !localUri.Host.Equals(fallbackUri.Host, StringComparison.OrdinalIgnoreCase) ||
                localUri.Port != fallbackUri.Port)
                return null;

            var fileName = Path.GetFileName(Uri.UnescapeDataString(localUri.AbsolutePath));
            var uploadsRoot = Path.GetFullPath(Path.Combine(_hostingEnv.ContentRootPath, "wwwroot", "uploads"));
            var filePath = Path.GetFullPath(Path.Combine(uploadsRoot, fileName));
            if (!filePath.StartsWith(uploadsRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
                !File.Exists(filePath))
                return null;

            await using var localFile = File.OpenRead(filePath);
            return await ReadCappedAsync(localFile, maximumBytes, cancellationToken);
        }

        if (!Uri.TryCreate(fileUrl, UriKind.Absolute, out var uri))
            return null;

        var serviceClient = _blobServiceClient.Value;
        var source = new BlobUriBuilder(uri);
        if (!string.Equals(uri.Host, serviceClient.Uri.Host, StringComparison.OrdinalIgnoreCase) ||
            !source.BlobContainerName.Equals(_settings.PrivateContainerName, StringComparison.OrdinalIgnoreCase) ||
            string.IsNullOrWhiteSpace(source.BlobName))
        {
            _logger.LogWarning("Refusing to download a private image from an unexpected blob URL.");
            return null;
        }

        var blobClient = serviceClient
            .GetBlobContainerClient(source.BlobContainerName)
            .GetBlobClient(source.BlobName);
        var download = await blobClient.DownloadStreamingAsync(cancellationToken: cancellationToken);
        if (download.Value.Details.ContentLength > maximumBytes)
            return null;

        await using var content = download.Value.Content;
        return await ReadCappedAsync(content, maximumBytes, cancellationToken);
    }

    public async Task DeleteFileAsync(string fileUrl)
    {
        if (string.IsNullOrEmpty(fileUrl)) return;

        try
        {
            if (fileUrl.Contains("/uploads/"))
            {
                var fileName = Path.GetFileName(fileUrl.Substring(fileUrl.LastIndexOf("/uploads/") + "/uploads/".Length));
                var uploadsDir = Path.Combine(_hostingEnv.ContentRootPath, "wwwroot", "uploads");
                var filePath = Path.GetFullPath(Path.Combine(uploadsDir, fileName));

                if (!filePath.StartsWith(Path.GetFullPath(uploadsDir) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("The local upload path is outside the configured upload directory.");

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    _logger.LogInformation("Deleted old local file: {FilePath}", filePath);
                }
            }
            else
            {
                if (Uri.TryCreate(fileUrl, UriKind.Absolute, out var uri))
                {
                    var blobServiceClient = _blobServiceClient.Value;
                    var source = new BlobUriBuilder(uri);
                    if (!string.Equals(uri.Host, blobServiceClient.Uri.Host, StringComparison.OrdinalIgnoreCase) ||
                        !IsAllowedContainer(source.BlobContainerName))
                        throw new InvalidOperationException("The blob URL is outside the configured storage containers.");
                    var containerClient = blobServiceClient.GetBlobContainerClient(source.BlobContainerName);
                    var blobClient = containerClient.GetBlobClient(source.BlobName);
                    
                    var deleted = await blobClient.DeleteIfExistsAsync();
                    if (deleted)
                    {
                        _logger.LogInformation("Deleted old Azure blob: {BlobName}", source.BlobName);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file from storage: {FileUrl}", fileUrl);
        }
    }

    private bool IsAllowedContainer(string containerName) =>
        containerName.Equals(_settings.ContainerName, StringComparison.OrdinalIgnoreCase) ||
        containerName.Equals(_settings.PrivateContainerName, StringComparison.OrdinalIgnoreCase);

    private bool IsPrivateBlobUrl(string fileUrl)
    {
        try
        {
            return Uri.TryCreate(fileUrl, UriKind.Absolute, out var uri) &&
                   new BlobUriBuilder(uri).BlobContainerName.Equals(
                       _settings.PrivateContainerName,
                       StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private static async Task<byte[]?> ReadCappedAsync(
        Stream source,
        int maximumBytes,
        CancellationToken cancellationToken)
    {
        using var destination = new MemoryStream();
        var buffer = new byte[81920];
        while (true)
        {
            var read = await source.ReadAsync(buffer, cancellationToken);
            if (read == 0)
                return destination.ToArray();
            if (destination.Length + read > maximumBytes)
                return null;
            await destination.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
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
        var baseUrl = !string.IsNullOrEmpty(_settings.LocalFallbackBaseUrl)
            ? _settings.LocalFallbackBaseUrl.TrimEnd('/')
            : "http://localhost:7000";
        return $"{baseUrl}/uploads/{uniqueFileName}";
    }
}
