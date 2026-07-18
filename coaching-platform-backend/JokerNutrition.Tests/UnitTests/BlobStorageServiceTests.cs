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
    public async Task UploadPrivatePhoto_WhenAzureIsUnavailable_FailsClosedWithoutPublicFallback()
    {
        var contentRoot = Path.Combine(Path.GetTempPath(), $"joker-private-upload-{Guid.NewGuid():N}");
        var service = new BlobStorageService(
            Options.Create(new BlobStorageSettings
            {
                ConnectionString = "not-a-valid-azure-connection-string",
                ContainerName = "public-assets",
                PrivateContainerName = "athlete-progress-photos",
                LocalFallbackBaseUrl = "https://public.example.test"
            }),
            new TestWebHostEnvironment(contentRoot),
            NullLogger<BlobStorageService>.Instance);

        await using var photo = new MemoryStream([1, 2, 3, 4]);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UploadPrivatePhotoAsync(photo, "progress.jpg", "image/jpeg"));

        Assert.Equal("Private photo storage is temporarily unavailable.", exception.Message);
        Assert.False(Directory.Exists(Path.Combine(contentRoot, "wwwroot", "uploads")));
    }

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
