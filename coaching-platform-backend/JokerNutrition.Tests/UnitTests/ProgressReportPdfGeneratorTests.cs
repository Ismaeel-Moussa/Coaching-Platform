using System.Text;
using JokerNutrition.Business.DTOs.Coach;
using JokerNutrition.Business.Reports;
using JokerNutrition.Business.Services;
using Microsoft.Extensions.Logging.Abstractions;
using SkiaSharp;
using Xunit;

namespace JokerNutrition.Tests.UnitTests;

public class ProgressReportPdfGeneratorTests
{
    [Fact]
    public async Task GenerateAsync_PrivatePhotoDownloadedFromStorage_EmbedsImageInPdf()
    {
        using var source = new SKBitmap(320, 240, SKColorType.Rgba8888, SKAlphaType.Premul);
        source.Erase(new SKColor(35, 90, 160));
        using var image = SKImage.FromBitmap(source);
        using var encoded = image.Encode(SKEncodedImageFormat.Jpeg, 90);
        var blobStorage = new StubBlobStorageService(encoded.ToArray());
        var generator = new ProgressReportPdfGenerator(
            blobStorage,
            NullLogger<ProgressReportPdfGenerator>.Instance);
        var report = new AthleteProgressReportDto
        {
            AthleteId = 1,
            AthleteName = "Test Athlete",
            PeriodStart = "2026-07-01",
            PeriodEnd = "2026-07-18",
            GeneratedAt = "2026-07-18T12:00:00Z",
            Weeks = 4,
            ProgressPhotos =
            [
                new ProgressReportPhotoDto
                {
                    Id = 7,
                    WeekOf = "2026-07-13",
                    Angle = "Front",
                    Url = "https://storage.example.test/athlete-progress-photos/front.jpg?sig=test"
                }
            ]
        };

        var pdf = await generator.GenerateAsync(
            report,
            includeCoachNotes: false,
            includePhotos: true,
            language: "en");

        Assert.Equal(report.ProgressPhotos[0].Url, blobStorage.RequestedUrl);
        Assert.Contains("/Subtype /Image", Encoding.Latin1.GetString(pdf));
    }

    [Fact]
    public void PreparePhoto_HighResolutionPhoneJpeg_DownsamplesForPdf()
    {
        using var source = new SKBitmap(7_000, 100, SKColorType.Rgba8888, SKAlphaType.Premul);
        source.Erase(new SKColor(35, 90, 160));
        using var image = SKImage.FromBitmap(source);
        using var encoded = image.Encode(SKEncodedImageFormat.Jpeg, 90);

        var prepared = ProgressReportPdfGenerator.PreparePhoto(encoded.ToArray());

        Assert.NotNull(prepared);
        Assert.InRange(prepared.Length, 1, 2 * 1024 * 1024);
        using var preparedData = SKData.CreateCopy(prepared);
        using var codec = SKCodec.Create(preparedData);
        Assert.NotNull(codec);
        Assert.True(Math.Max(codec.Info.Width, codec.Info.Height) <= 1_400);
    }

    private sealed class StubBlobStorageService(byte[] imageBytes) : IBlobStorageService
    {
        public string? RequestedUrl { get; private set; }

        public Task<byte[]?> DownloadPrivateImageAsync(
            string fileUrl,
            int maximumBytes,
            CancellationToken cancellationToken = default)
        {
            RequestedUrl = fileUrl;
            return Task.FromResult<byte[]?>(imageBytes.Length <= maximumBytes ? imageBytes : null);
        }

        public Task<string> UploadPublicAssetAsync(Stream fileStream, string fileName, string contentType) =>
            throw new NotSupportedException();

        public Task<string> UploadPrivatePhotoAsync(Stream fileStream, string fileName, string contentType) =>
            throw new NotSupportedException();

        public Task<string> GetReadUrlAsync(string fileUrl, TimeSpan? lifetime = null) =>
            throw new NotSupportedException();

        public Task DeleteFileAsync(string fileUrl) => throw new NotSupportedException();
    }
}
