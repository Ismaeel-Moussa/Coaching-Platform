using System.Globalization;
using JokerNutrition.Business.DTOs.Coach;
using JokerNutrition.Business.Services;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Drawing;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SkiaSharp;

namespace JokerNutrition.Business.Reports;

public interface IProgressReportPdfGenerator
{
    Task<byte[]> GenerateAsync(
        AthleteProgressReportDto report,
        bool includeCoachNotes,
        bool includePhotos,
        string language,
        CancellationToken cancellationToken = default);
}

public class ProgressReportPdfGenerator : IProgressReportPdfGenerator
{
    private const int MaximumPhotoBytes = 10 * 1024 * 1024;
    private const int MaximumSourcePhotoDimension = 20_000;
    private const long MaximumSourcePhotoPixels = 120_000_000;
    private const long MaximumDecodedPhotoPixels = 24_000_000;
    private const int MaximumPreparedPhotoDimension = 1400;
    private const int MaximumPreparedPhotoBytes = 2 * 1024 * 1024;
    private const string FontFamily = "Joker Report Sans";
    private const string Navy = "#0B132B";
    private const string Gold = "#FDC003";
    private const string Border = "#D8DCE7";
    private const string Muted = "#667085";
    private const string Soft = "#F7F8FC";
    private static readonly object FontLock = new();
    private static bool _fontsRegistered;
    private readonly IBlobStorageService _blobStorage;
    private readonly ILogger<ProgressReportPdfGenerator> _logger;

    public ProgressReportPdfGenerator(
        IBlobStorageService blobStorage,
        ILogger<ProgressReportPdfGenerator> logger)
    {
        _blobStorage = blobStorage;
        _logger = logger;
    }

    public async Task<byte[]> GenerateAsync(
        AthleteProgressReportDto report,
        bool includeCoachNotes,
        bool includePhotos,
        string language,
        CancellationToken cancellationToken = default)
    {
        ConfigureQuestPdf();
        var photoSources = includePhotos
            ? await DownloadPhotosAsync(report.ProgressPhotos, cancellationToken)
            : new Dictionary<int, byte[]>();
        cancellationToken.ThrowIfCancellationRequested();

        var isArabic = language.Equals("ar", StringComparison.OrdinalIgnoreCase);
        var culture = CultureInfo.GetCultureInfo(isArabic ? "ar" : "en");
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.MarginHorizontal(32);
                page.MarginVertical(24);
                page.DefaultTextStyle(style => style.FontFamily(FontFamily).FontSize(9).FontColor(Navy));
                if (isArabic)
                    page.ContentFromRightToLeft();

                page.Header().Element(header => ComposeHeader(header, isArabic));
                page.Content().PaddingVertical(10).Column(column =>
                {
                    column.Spacing(12);
                    ComposeIdentity(column, report, isArabic, culture);
                    ComposeOverview(column, report, isArabic);
                    ComposeSummary(column, report.Summary, isArabic);
                    ComposeWeeklyProgress(column, report.WeeklyProgress, isArabic, culture);
                    ComposeCheckIns(column, report.CheckIns, includeCoachNotes, isArabic, culture);

                    if (includeCoachNotes && report.CoachNotes.Count > 0)
                        ComposeCoachNotes(column, report.CoachNotes, isArabic, culture);

                    if (photoSources.Count > 0)
                        ComposeProgressPhotos(column, report.ProgressPhotos, photoSources, isArabic, culture);

                    column.Item().PaddingTop(6).Text(
                            $"{L(isArabic, "تم الإنشاء", "Generated")} {FormatDateTime(report.GeneratedAt, culture)}")
                        .FontSize(7).FontColor(Muted);
                });
                page.Footer().AlignCenter().Text(text =>
                {
                    text.DefaultTextStyle(style => style.FontFamily(FontFamily).FontSize(7).FontColor(Muted));
                    text.Span(L(isArabic, "تقرير تدريب خاص - صفحة ", "Sensitive coaching report - Page "));
                    text.CurrentPageNumber();
                    text.Span(L(isArabic, " من ", " of "));
                    text.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }

    private static void ConfigureQuestPdf()
    {
        QuestPDF.Settings.License = LicenseType.Community;
        if (_fontsRegistered)
            return;

        lock (FontLock)
        {
            if (_fontsRegistered)
                return;

            var assembly = typeof(ProgressReportPdfGenerator).Assembly;
            using var regular = assembly.GetManifestResourceStream("JokerNutrition.Business.Assets.Fonts.DejaVuSans.ttf")
                ?? throw new InvalidOperationException("Embedded regular PDF font was not found.");
            using var bold = assembly.GetManifestResourceStream("JokerNutrition.Business.Assets.Fonts.DejaVuSans-Bold.ttf")
                ?? throw new InvalidOperationException("Embedded bold PDF font was not found.");
            FontManager.RegisterFontWithCustomName(FontFamily, regular);
            FontManager.RegisterFontWithCustomName(FontFamily, bold);
            _fontsRegistered = true;
        }
    }

    private static void ComposeHeader(IContainer container, bool isArabic)
    {
        container.BorderBottom(1).BorderColor(Gold).PaddingBottom(6).Row(row =>
        {
            row.RelativeItem().Text(L(isArabic, "جوكر نيوترشن", "JOKER NUTRITION")).Bold();
            row.RelativeItem().AlignRight().Text(L(isArabic, "منصة التدريب", "COACHING PLATFORM"))
                .FontSize(7).FontColor(Muted);
        });
    }

    private static void ComposeIdentity(
        ColumnDescriptor column,
        AthleteProgressReportDto report,
        bool isArabic,
        CultureInfo culture)
    {
        column.Item().Text(L(isArabic, "تقرير تقدم المتدرب", "ATHLETE PROGRESS REPORT"))
            .Bold().FontSize(8).FontColor("#8A6800");
        column.Item().Text(string.IsNullOrWhiteSpace(report.AthleteName)
                ? $"{L(isArabic, "المتدرب", "Athlete")} {report.AthleteId}"
                : report.AthleteName.Trim())
            .Bold().FontSize(22);
        column.Item().Text(
                $"{report.Weeks} {L(isArabic, "أسابيع", "weeks")} | {FormatDate(report.PeriodStart, culture)} - {FormatDate(report.PeriodEnd, culture)}")
            .FontColor(Muted);
    }

    private static void ComposeOverview(ColumnDescriptor column, AthleteProgressReportDto report, bool isArabic)
    {
        column.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
            });
            OverviewCell(table.Cell(), L(isArabic, "الهدف", "Goal"),
                string.IsNullOrWhiteSpace(report.TargetGoal) ? L(isArabic, "غير محدد", "Not specified") : report.TargetGoal);
            OverviewCell(table.Cell(), L(isArabic, "الطول", "Height"),
                report.HeightCm.HasValue ? $"{report.HeightCm:0.#} cm" : L(isArabic, "لا توجد بيانات", "No data"));
            OverviewCell(table.Cell(), L(isArabic, "الوزن الحالي", "Current weight"),
                FormatWeight(report.Summary.CurrentWeightKg, isArabic));
        });
    }

    private static void OverviewCell(IContainer container, string label, string value)
    {
        container.Border(0.7f).BorderColor(Border).Background(Soft).Padding(9).Column(column =>
        {
            column.Item().Text(label).FontSize(7).FontColor(Muted);
            column.Item().PaddingTop(2).Text(value).Bold().FontSize(10);
        });
    }

    private static void ComposeSummary(ColumnDescriptor column, ProgressReportSummaryDto summary, bool isArabic)
    {
        SectionTitle(column, L(isArabic, "ملخص الأداء", "Performance summary"));
        column.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
            });
            MetricCell(table.Cell(), FormatSignedWeight(summary.WeightChangeKg, isArabic), L(isArabic, "تغير الوزن", "Weight change"));
            MetricCell(table.Cell(), $"{summary.CompletedWorkouts}/{summary.LoggedWorkouts}", L(isArabic, "الحصص المكتملة", "Sessions completed"));
            MetricCell(table.Cell(), FormatPercent(summary.AverageCalorieAdherencePercent, isArabic), L(isArabic, "الالتزام بالسعرات", "Calorie adherence"));
            MetricCell(table.Cell(), summary.CheckInCount.ToString(CultureInfo.InvariantCulture), L(isArabic, "المتابعات", "Check-ins"));
        });
    }

    private static void MetricCell(IContainer container, string value, string label)
    {
        container.Border(0.7f).BorderColor(Border).Padding(8).Column(column =>
        {
            column.Item().ContentFromLeftToRight().Text(value).Bold().FontSize(13);
            column.Item().PaddingTop(2).Text(label).FontSize(7).FontColor(Muted);
        });
    }

    private static void ComposeWeeklyProgress(
        ColumnDescriptor column,
        IReadOnlyCollection<ProgressReportWeekDto> weeks,
        bool isArabic,
        CultureInfo culture)
    {
        SectionTitle(column, L(isArabic, "التقدم الأسبوعي", "Weekly progress"));
        column.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(1.25f);
                columns.RelativeColumn();
                columns.RelativeColumn(1.3f);
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
            });
            table.Header(header =>
            {
                TableHeader(header.Cell(), L(isArabic, "الأسبوع", "Week"));
                TableHeader(header.Cell(), L(isArabic, "الوزن", "Weight"));
                TableHeader(header.Cell(), L(isArabic, "التمارين", "Workouts"));
                TableHeader(header.Cell(), L(isArabic, "السعرات", "Calories"));
                TableHeader(header.Cell(), L(isArabic, "البروتين", "Protein"));
                TableHeader(header.Cell(), L(isArabic, "الخطوات", "Steps"));
            });

            foreach (var week in weeks.OrderByDescending(week => week.WeekOf))
            {
                TableBody(table.Cell(), FormatDate(week.WeekOf, culture));
                TableBody(table.Cell(), FormatWeight(week.WeightKg, isArabic));
                TableBody(table.Cell(), week.LoggedWorkouts > 0
                    ? $"{week.CompletedWorkouts}/{week.LoggedWorkouts} ({FormatPercent(week.WorkoutCompletionPercent, isArabic)})"
                    : L(isArabic, "لا توجد سجلات", "No logs"));
                TableBody(table.Cell(), FormatPercent(week.CalorieAdherencePercent, isArabic));
                TableBody(table.Cell(), FormatPercent(week.ProteinAdherencePercent, isArabic));
                TableBody(table.Cell(), FormatPercent(week.StepsAdherencePercent, isArabic));
            }
        });
    }

    private static void ComposeCheckIns(
        ColumnDescriptor column,
        IReadOnlyCollection<ProgressReportCheckInDto> checkIns,
        bool includeCoachNotes,
        bool isArabic,
        CultureInfo culture)
    {
        SectionTitle(column, L(isArabic, "سجل المتابعة", "Check-in history"));
        if (checkIns.Count == 0)
        {
            column.Item().Text(L(isArabic, "لم يتم إرسال متابعات خلال هذه الفترة.", "No check-ins were submitted during this period."));
            return;
        }

        column.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(1.25f);
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn(1.3f);
                columns.RelativeColumn(1.8f);
            });
            table.Header(header =>
            {
                TableHeader(header.Cell(), L(isArabic, "الأسبوع", "Week"));
                TableHeader(header.Cell(), L(isArabic, "الوزن", "Weight"));
                TableHeader(header.Cell(), L(isArabic, "الخصر", "Waist"));
                TableHeader(header.Cell(), L(isArabic, "الصدر / الفخذ", "Chest / thigh"));
                TableHeader(header.Cell(), L(isArabic, "الطاقة / النوم / الهضم / الإجهاد", "Energy / sleep / gut / stress"));
            });
            foreach (var checkIn in checkIns)
            {
                TableBody(table.Cell(), FormatDate(checkIn.WeekOf, culture));
                TableBody(table.Cell(), $"{checkIn.WeightKg:0.#} kg");
                TableBody(table.Cell(), FormatCentimeters(checkIn.WaistCm));
                TableBody(table.Cell(), $"{FormatCentimeters(checkIn.ChestCm)} / {FormatCentimeters(checkIn.ThighCm)}");
                TableBody(table.Cell(), $"{checkIn.EnergyLevel} / {checkIn.SleepQuality} / {checkIn.GutHealth} / {checkIn.TrainingStress}");
            }
        });

        if (!includeCoachNotes)
            return;

        foreach (var checkIn in checkIns.Where(checkIn => !string.IsNullOrWhiteSpace(checkIn.ReviewNotes)))
        {
            var noteContainer = isArabic
                ? column.Item().BorderRight(2).BorderColor(Gold).PaddingRight(8)
                : column.Item().BorderLeft(2).BorderColor(Gold).PaddingLeft(8);
            noteContainer.Text(text =>
            {
                text.Span($"{FormatDate(checkIn.WeekOf, culture)}: ").Bold();
                text.Span(checkIn.ReviewNotes!.Trim());
            });
        }
    }

    private static void ComposeCoachNotes(
        ColumnDescriptor column,
        IReadOnlyCollection<ProgressReportNoteDto> notes,
        bool isArabic,
        CultureInfo culture)
    {
        SectionTitle(column, L(isArabic, "ملاحظات المدرب", "Coach notes"));
        foreach (var note in notes)
        {
            var noteContainer = isArabic
                ? column.Item().BorderRight(2).BorderColor(Gold).PaddingRight(8)
                : column.Item().BorderLeft(2).BorderColor(Gold).PaddingLeft(8);
            noteContainer.Column(noteColumn =>
            {
                noteColumn.Item().Text($"{note.CoachName} - {FormatDate(note.CreatedAt[..10], culture)}").Bold();
                noteColumn.Item().PaddingTop(2).Text(note.Text);
            });
        }
    }

    private static void ComposeProgressPhotos(
        ColumnDescriptor column,
        IReadOnlyCollection<ProgressReportPhotoDto> photos,
        IReadOnlyDictionary<int, byte[]> photoSources,
        bool isArabic,
        CultureInfo culture)
    {
        column.Item().PageBreak();
        SectionTitle(column, L(isArabic, "صور التقدم", "Progress photos"));
        column.Item().Text(L(
            isArabic,
            "أقدم وأحدث الصور المتاحة خلال فترة التقرير.",
            "Earliest and latest photos available in the reporting period.")).FontColor(Muted);

        var selected = photos.Where(photo => photoSources.ContainsKey(photo.Id)).ToList();
        column.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
            });
            foreach (var photo in selected)
            {
                table.Cell().Border(0.7f).BorderColor(Border).Padding(6).Column(cell =>
                {
                    cell.Item().Height(220).Image(photoSources[photo.Id]).FitArea();
                    cell.Item().PaddingTop(4).AlignCenter().Text(
                            $"{TranslateAngle(photo.Angle, isArabic)} - {FormatDate(photo.WeekOf, culture)}")
                        .FontSize(7).FontColor(Muted);
                });
            }
        });
    }

    private static void SectionTitle(ColumnDescriptor column, string title) =>
        column.Item().PaddingTop(4).Text(title).Bold().FontSize(13);

    private static void TableHeader(IContainer container, string value) =>
        container.Background(Navy).Padding(5).AlignMiddle().Text(value).Bold().FontSize(7).FontColor(Colors.White);

    private static void TableBody(IContainer container, string value)
    {
        var cell = container.BorderBottom(0.5f).BorderColor(Border).PaddingVertical(5).PaddingHorizontal(3).AlignMiddle();
        if (!value.Any(IsArabicCharacter))
            cell = cell.ContentFromLeftToRight();
        cell.Text(value).FontSize(7);
    }

    private async Task<Dictionary<int, byte[]>> DownloadPhotosAsync(
        IReadOnlyCollection<ProgressReportPhotoDto> photos,
        CancellationToken cancellationToken)
    {
        var result = new System.Collections.Concurrent.ConcurrentDictionary<int, byte[]>();
        using var totalTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        totalTimeout.CancelAfter(TimeSpan.FromSeconds(30));
        using var concurrency = new SemaphoreSlim(2);

        var tasks = photos.Take(6).Select(async photo =>
        {
            var entered = false;
            try
            {
                await concurrency.WaitAsync(totalTimeout.Token);
                entered = true;
                using var requestTimeout = CancellationTokenSource.CreateLinkedTokenSource(totalTimeout.Token);
                requestTimeout.CancelAfter(TimeSpan.FromSeconds(15));
                var bytes = await _blobStorage.DownloadPrivateImageAsync(
                    photo.Url,
                    MaximumPhotoBytes,
                    requestTimeout.Token);
                if (bytes is null)
                {
                    _logger.LogWarning("Progress photo {PhotoId} could not be downloaded for the PDF.", photo.Id);
                    return;
                }

                if (PreparePhoto(bytes) is { } preparedPhoto)
                    result.TryAdd(photo.Id, preparedPhoto);
                else
                    _logger.LogWarning(
                        "Progress photo {PhotoId} could not be decoded safely for the PDF.",
                        photo.Id);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Could not include progress photo {PhotoId} in PDF.", photo.Id);
            }
            finally
            {
                if (entered)
                    concurrency.Release();
            }
        });

        await Task.WhenAll(tasks);
        cancellationToken.ThrowIfCancellationRequested();
        return result.ToDictionary(entry => entry.Key, entry => entry.Value);
    }

    internal static byte[]? PreparePhoto(byte[] bytes)
    {
        try
        {
            using var data = SKData.CreateCopy(bytes);
            using var codec = SKCodec.Create(data);
            if (codec is null || codec.Info.Width <= 0 || codec.Info.Height <= 0 ||
                codec.Info.Width > MaximumSourcePhotoDimension ||
                codec.Info.Height > MaximumSourcePhotoDimension ||
                (long)codec.Info.Width * codec.Info.Height > MaximumSourcePhotoPixels)
                return null;

            var scale = Math.Min(1f, MaximumPreparedPhotoDimension / (float)Math.Max(codec.Info.Width, codec.Info.Height));
            var width = Math.Max(1, (int)Math.Round(codec.Info.Width * scale));
            var height = Math.Max(1, (int)Math.Round(codec.Info.Height * scale));
            var supportedSize = codec.GetScaledDimensions(scale);
            if (supportedSize.Width <= 0 || supportedSize.Height <= 0 ||
                (long)supportedSize.Width * supportedSize.Height > MaximumDecodedPhotoPixels)
                return null;

            var decodeInfo = new SKImageInfo(
                supportedSize.Width,
                supportedSize.Height,
                SKColorType.Rgba8888,
                SKAlphaType.Premul);
            using var decoded = new SKBitmap(decodeInfo);
            var decodeResult = codec.GetPixels(decodeInfo, decoded.GetPixels());
            if (decodeResult != SKCodecResult.Success)
                return null;

            using var resized = ResizeBitmap(decoded, width, height);
            using var prepared = ApplyOrientation(resized, codec.EncodedOrigin);
            using var image = SKImage.FromBitmap(prepared);
            using var encoded = image.Encode(SKEncodedImageFormat.Jpeg, 84);
            var output = encoded?.ToArray();
            return output is { Length: > 0 and <= MaximumPreparedPhotoBytes } ? output : null;
        }
        catch
        {
            return null;
        }
    }

    private static SKBitmap ResizeBitmap(SKBitmap source, int width, int height)
    {
        var output = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var canvas = new SKCanvas(output);
        canvas.Clear(SKColors.White);
        canvas.DrawBitmap(
            source,
            new SKRect(0, 0, width, height),
            new SKSamplingOptions(SKCubicResampler.Mitchell),
            null);
        canvas.Flush();
        return output;
    }

    private static SKBitmap ApplyOrientation(SKBitmap source, SKEncodedOrigin origin)
    {
        var swapsAxes = origin is SKEncodedOrigin.LeftTop or SKEncodedOrigin.RightTop or
            SKEncodedOrigin.RightBottom or SKEncodedOrigin.LeftBottom;
        var outputWidth = swapsAxes ? source.Height : source.Width;
        var outputHeight = swapsAxes ? source.Width : source.Height;
        var output = new SKBitmap(outputWidth, outputHeight, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var canvas = new SKCanvas(output);
        canvas.Clear(SKColors.White);

        var matrix = origin switch
        {
            SKEncodedOrigin.TopRight => CreateMatrix(-1, 0, source.Width, 0, 1, 0),
            SKEncodedOrigin.BottomRight => CreateMatrix(-1, 0, source.Width, 0, -1, source.Height),
            SKEncodedOrigin.BottomLeft => CreateMatrix(1, 0, 0, 0, -1, source.Height),
            SKEncodedOrigin.LeftTop => CreateMatrix(0, 1, 0, 1, 0, 0),
            SKEncodedOrigin.RightTop => CreateMatrix(0, -1, source.Height, 1, 0, 0),
            SKEncodedOrigin.RightBottom => CreateMatrix(0, -1, source.Height, -1, 0, source.Width),
            SKEncodedOrigin.LeftBottom => CreateMatrix(0, 1, 0, -1, 0, source.Width),
            _ => SKMatrix.CreateIdentity()
        };
        canvas.SetMatrix(matrix);
        canvas.DrawBitmap(
            source,
            0,
            0,
            new SKSamplingOptions(SKCubicResampler.Mitchell),
            null);
        canvas.Flush();
        return output;
    }

    private static SKMatrix CreateMatrix(
        float scaleX,
        float skewX,
        float transX,
        float skewY,
        float scaleY,
        float transY) => new()
    {
        ScaleX = scaleX,
        SkewX = skewX,
        TransX = transX,
        SkewY = skewY,
        ScaleY = scaleY,
        TransY = transY,
        Persp2 = 1
    };

    private static string L(bool isArabic, string arabic, string english) => isArabic ? arabic : english;

    private static string FormatDate(string value, CultureInfo culture) =>
        DateOnly.TryParse(value, out var date) ? date.ToString("dd MMM yyyy", culture) : value;

    private static string FormatDateTime(string value, CultureInfo culture) =>
        DateTime.TryParse(value, out var date) ? date.ToUniversalTime().ToString("dd MMM yyyy, HH:mm 'UTC'", culture) : value;

    private static string FormatWeight(decimal? value, bool isArabic) =>
        value.HasValue ? $"{value:0.#} kg" : L(isArabic, "لا توجد بيانات", "No data");

    private static string FormatCentimeters(decimal? value) => value.HasValue ? $"{value:0.#} cm" : "-";

    private static string FormatSignedWeight(decimal? value, bool isArabic) =>
        value.HasValue ? $"{(value.Value > 0 ? "+" : string.Empty)}{value.Value:0.#} kg" : L(isArabic, "لا توجد بيانات", "No data");

    private static string FormatPercent(double? value, bool isArabic) =>
        value.HasValue ? $"{value:0.#}%" : L(isArabic, "لا توجد بيانات", "No data");

    private static string TranslateAngle(string angle, bool isArabic) => angle.ToLowerInvariant() switch
    {
        "front" => L(isArabic, "أمامي", "Front"),
        "side" => L(isArabic, "جانبي", "Side"),
        "back" => L(isArabic, "خلفي", "Back"),
        _ => angle
    };

    private static bool IsArabicCharacter(char character) =>
        character is >= '\u0600' and <= '\u06FF' or
        >= '\u0750' and <= '\u077F' or
        >= '\u08A0' and <= '\u08FF' or
        >= '\uFB50' and <= '\uFDFF' or
        >= '\uFE70' and <= '\uFEFF';
}
