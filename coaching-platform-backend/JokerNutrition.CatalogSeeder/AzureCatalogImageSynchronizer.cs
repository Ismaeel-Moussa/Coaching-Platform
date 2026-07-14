using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using JokerNutrition.Data.Seeding;

namespace JokerNutrition.CatalogSeeder;

internal sealed record CatalogImageSyncResult(int ImageCount, long TotalBytes, bool DryRun);

internal sealed class AzureCatalogImageSynchronizer
{
    private readonly BlobContainerClient _container;
    private readonly string? _publicBaseUrl;

    public AzureCatalogImageSynchronizer(
        string connectionString,
        string containerName,
        string? publicBaseUrl = null)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Azure Blob connection string is required.", nameof(connectionString));
        if (string.IsNullOrWhiteSpace(containerName))
            throw new ArgumentException("Azure Blob container name is required.", nameof(containerName));

        _container = new BlobContainerClient(connectionString, containerName);
        _publicBaseUrl = string.IsNullOrWhiteSpace(publicBaseUrl) ? null : publicBaseUrl;
    }

    public async Task<CatalogImageSyncResult> SynchronizeAsync(
        CoachCatalogPackage package,
        string catalogDirectory,
        bool dryRun,
        CancellationToken cancellationToken = default)
    {
        var properties = await _container.GetPropertiesAsync(cancellationToken: cancellationToken);
        if (properties.Value.PublicAccess == PublicAccessType.None && _publicBaseUrl is null)
        {
            throw new InvalidOperationException(
                "The configured Azure container is private. Set BlobStorageSettings__PublicBaseUrl " +
                "to the public CDN/proxy base URL used by the frontend.");
        }

        var root = Path.GetFullPath(catalogDirectory);
        var assets = package.Manifest.Assets.ToDictionary(
            asset => asset.Path,
            StringComparer.OrdinalIgnoreCase);
        var imageCount = 0;
        long totalBytes = 0;

        foreach (var recipe in package.Recipes.Where(recipe => !string.IsNullOrWhiteSpace(recipe.ImageAssetPath)))
        {
            if (!assets.TryGetValue(recipe.ImageAssetPath!, out var asset))
                throw new InvalidDataException(
                    $"Recipe '{recipe.SeedKey}' references unmanifested asset '{recipe.ImageAssetPath}'.");
            if (!asset.ContentType.Equals("image/webp", StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException(
                    $"Recipe asset '{asset.Path}' must use image/webp, not '{asset.ContentType}'.");

            var localPath = ResolveAssetPath(root, asset.Path);
            var file = new FileInfo(localPath);
            totalBytes += file.Length;
            imageCount++;

            var blobName = $"coach-catalog/recipes/{recipe.SeedKey}.v{recipe.ContentVersion}.webp";
            var blob = _container.GetBlobClient(blobName);
            if (!dryRun)
            {
                await using var stream = File.OpenRead(localPath);
                await blob.UploadAsync(
                    stream,
                    new BlobUploadOptions
                    {
                        HttpHeaders = new BlobHttpHeaders
                        {
                            ContentType = "image/webp",
                            CacheControl = "public, max-age=31536000, immutable"
                        },
                        Metadata = new Dictionary<string, string>
                        {
                            ["seedkey"] = recipe.SeedKey,
                            ["contentversion"] = recipe.ContentVersion.ToString(),
                            ["catalogversion"] = package.Manifest.CatalogVersion,
                            ["sha256"] = asset.Sha256
                        }
                    },
                    cancellationToken);
            }

            recipe.ImageUrl = BuildPublicUrl(blob, blobName);
        }

        return new CatalogImageSyncResult(imageCount, totalBytes, dryRun);
    }

    private string BuildPublicUrl(BlobClient blob, string blobName)
    {
        if (_publicBaseUrl is null) return blob.Uri.AbsoluteUri;
        var escapedPath = string.Join('/', blobName.Split('/').Select(Uri.EscapeDataString));
        return $"{_publicBaseUrl.TrimEnd('/')}/{escapedPath}";
    }

    private static string ResolveAssetPath(string root, string relativePath)
    {
        var path = Path.GetFullPath(Path.Combine(root, relativePath));
        var relative = Path.GetRelativePath(root, path);
        if (relative.Equals("..", StringComparison.Ordinal) ||
            relative.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            throw new InvalidDataException($"Catalog asset '{relativePath}' escapes the catalog directory.");
        return path;
    }
}
