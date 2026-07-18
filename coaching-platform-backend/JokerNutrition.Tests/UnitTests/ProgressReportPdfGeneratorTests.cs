using JokerNutrition.Business.Reports;
using SkiaSharp;
using Xunit;

namespace JokerNutrition.Tests.UnitTests;

public class ProgressReportPdfGeneratorTests
{
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
}
