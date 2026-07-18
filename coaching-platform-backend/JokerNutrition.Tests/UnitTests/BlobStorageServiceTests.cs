using JokerNutrition.Business.Configurations;
using JokerNutrition.Business.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace JokerNutrition.Tests.UnitTests;

public class BlobStorageServiceTests
{
    [Fact]
    public async Task DownloadPrivateImage_LocalFallback_ReadsExistingImageWithinLimit()
    {
        var contentRoot = Path.Combine(Path.GetTempPath(), $"joker-private-download-{Guid.NewGuid():N}");
        var uploadsRoot = Path.Combine(contentRoot, "wwwroot", "uploads");
        Directory.CreateDirectory(uploadsRoot);
        var expected = new byte[] { 1, 2, 3, 4 };
        await File.WriteAllBytesAsync(Path.Combine(uploadsRoot, "progress.jpg"), expected);
        var service = CreateService(contentRoot);

        try
        {
            var result = await service.DownloadPrivateImageAsync(
                "https://public.example.test/uploads/progress.jpg",
                maximumBytes: 10);

            Assert.Equal(expected, result);
        }
        finally
        {
            Directory.Delete(contentRoot, recursive: true);
        }
    }

    [Fact]
    public async Task DownloadPrivateImage_LocalFallback_RejectsUnexpectedHost()
    {
        var contentRoot = Path.Combine(Path.GetTempPath(), $"joker-private-download-{Guid.NewGuid():N}");
        var uploadsRoot = Path.Combine(contentRoot, "wwwroot", "uploads");
        Directory.CreateDirectory(uploadsRoot);
        await File.WriteAllBytesAsync(Path.Combine(uploadsRoot, "progress.jpg"), [1, 2, 3, 4]);
        var service = CreateService(contentRoot);

        try
        {
            var result = await service.DownloadPrivateImageAsync(
                "https://attacker.example.test/uploads/progress.jpg",
                maximumBytes: 10);

            Assert.Null(result);
        }
        finally
        {
            Directory.Delete(contentRoot, recursive: true);
        }
    }

    [Fact]
    public async Task UploadPrivatePhoto_WhenAzureIsUnavailable_FailsClosedWithoutPublicFallback()
    {
        var contentRoot = Path.Combine(Path.GetTempPath(), $"joker-private-upload-{Guid.NewGuid():N}");
        var service = CreateService(contentRoot);

        await using var photo = new MemoryStream([1, 2, 3, 4]);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UploadPrivatePhotoAsync(photo, "progress.jpg", "image/jpeg"));

        Assert.Equal("Private photo storage is temporarily unavailable.", exception.Message);
        Assert.False(Directory.Exists(Path.Combine(contentRoot, "wwwroot", "uploads")));
    }

    private static BlobStorageService CreateService(string contentRoot) =>
        new(
            Options.Create(new BlobStorageSettings
            {
                ConnectionString = "not-a-valid-azure-connection-string",
                ContainerName = "public-assets",
                PrivateContainerName = "athlete-progress-photos",
                LocalFallbackBaseUrl = "https://public.example.test"
            }),
            new TestWebHostEnvironment(contentRoot),
            NullLogger<BlobStorageService>.Instance);

    private sealed class TestWebHostEnvironment(string contentRoot) : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "JokerNutrition.Tests";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = Path.Combine(contentRoot, "wwwroot");
        public string EnvironmentName { get; set; } = "Test";
        public string ContentRootPath { get; set; } = contentRoot;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
