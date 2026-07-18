using Azure.Storage.Blobs;
using JokerNutrition.Business.Configurations;
using JokerNutrition.Business.DTOs.Coach;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PdfSharp.Drawing;

namespace JokerNutrition.Business.Reports;

public interface IProgressReportPdfGenerator
{
    Task<byte[]> GenerateAsync(
        AthleteProgressReportDto report,
        bool includeCoachNotes,
        bool includePhotos,
        CancellationToken cancellationToken = default);
}

public class ProgressReportPdfGenerator : IProgressReportPdfGenerator
{
    private const int MaximumPhotoBytes = 8 * 1024 * 1024;
    private const int MaximumPhotoDimension = 6000;
    private const long MaximumPhotoPixels = 24_000_000;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ProgressReportPdfGenerator> _logger;
    private readonly HashSet<string> _allowedPhotoHosts = new(StringComparer.OrdinalIgnoreCase);

    public ProgressReportPdfGenerator(
        IHttpClientFactory httpClientFactory,
        ILogger<ProgressReportPdfGenerator> logger,
        IOptions<BlobStorageSettings> blobOptions)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        var settings = blobOptions.Value;
        try
        {
            _allowedPhotoHosts.Add(new BlobServiceClient(settings.ConnectionString).Uri.Host);
        }
        catch (Exception exception)
        {
            _logger.LogDebug(exception, "Blob storage host could not be derived for PDF photo validation.");
        }
        if (Uri.TryCreate(settings.LocalFallbackBaseUrl, UriKind.Absolute, out var fallbackUri))
            _allowedPhotoHosts.Add(fallbackUri.Host);
    }

    public async Task<byte[]> GenerateAsync(
        AthleteProgressReportDto report,
        bool includeCoachNotes,
        bool includePhotos,
        CancellationToken cancellationToken = default)
    {
        ProgressReportFontResolver.EnsureConfigured();
        var photoSources = includePhotos
            ? await DownloadPhotosAsync(report.ProgressPhotos, cancellationToken)
            : new Dictionary<int, string>();
        cancellationToken.ThrowIfCancellationRequested();
        var document = BuildDocument(report, includeCoachNotes, photoSources);
        var renderer = new PdfDocumentRenderer { Document = document };
        renderer.RenderDocument();
        using var stream = new MemoryStream();
        renderer.PdfDocument.Save(stream, false);
        return stream.ToArray();
    }

    private static Document BuildDocument(
        AthleteProgressReportDto report,
        bool includeCoachNotes,
        IReadOnlyDictionary<int, string> photoSources)
    {
        var document = new Document();
        var athleteDisplayName = FormatPdfUserText(report.AthleteName, $"Athlete {report.AthleteId}");
        document.Info.Title = $"{athleteDisplayName} Progress Report";
        document.Info.Author = "Joker Nutrition";
        document.Info.Subject = $"{report.Weeks}-week athlete progress report";

        var normal = document.Styles[StyleNames.Normal]
            ?? throw new InvalidOperationException("The PDF normal style is unavailable.");
        normal.Font.Name = ProgressReportFontResolver.FamilyName;
        normal.Font.Size = 9;
        normal.Font.Color = Color.Parse("#141B2B");

        var heading1 = document.Styles[StyleNames.Heading1]
            ?? throw new InvalidOperationException("The PDF heading 1 style is unavailable.");
        heading1.Font.Name = ProgressReportFontResolver.FamilyName;
        heading1.Font.Bold = true;
        heading1.Font.Size = 22;
        heading1.Font.Color = Color.Parse("#0B132B");
        heading1.ParagraphFormat.SpaceAfter = Unit.FromMillimeter(2);

        var heading2 = document.Styles[StyleNames.Heading2]
            ?? throw new InvalidOperationException("The PDF heading 2 style is unavailable.");
        heading2.Font.Name = ProgressReportFontResolver.FamilyName;
        heading2.Font.Bold = true;
        heading2.Font.Size = 13;
        heading2.Font.Color = Color.Parse("#0B132B");
        heading2.ParagraphFormat.SpaceBefore = Unit.FromMillimeter(6);
        heading2.ParagraphFormat.SpaceAfter = Unit.FromMillimeter(3);
        heading2.ParagraphFormat.KeepWithNext = true;

        var section = document.AddSection();
        section.PageSetup.PageFormat = PageFormat.A4;
        section.PageSetup.TopMargin = Unit.FromMillimeter(22);
        section.PageSetup.BottomMargin = Unit.FromMillimeter(16);
        section.PageSetup.LeftMargin = Unit.FromMillimeter(18);
        section.PageSetup.RightMargin = Unit.FromMillimeter(18);

        AddHeader(section);
        AddFooter(section);

        var eyebrow = section.AddParagraph("ATHLETE PROGRESS REPORT");
        eyebrow.Format.SpaceAfter = Unit.FromMillimeter(1);
        eyebrow.Format.Font.Size = 8;
        eyebrow.Format.Font.Bold = true;
        eyebrow.Format.Font.Color = Color.Parse("#8A6800");

        section.AddParagraph(athleteDisplayName, StyleNames.Heading1);
        var period = section.AddParagraph(
            $"{report.Weeks} weeks  |  {FormatDate(report.PeriodStart)} - {FormatDate(report.PeriodEnd)}");
        period.Format.Font.Color = Color.Parse("#5E6472");
        period.Format.SpaceAfter = Unit.FromMillimeter(HasArabicContent(report) ? 1 : 5);

        if (HasArabicContent(report))
        {
            var languageNotice = section.AddParagraph(
                "Some Arabic source text is available only in the coaching platform and is omitted from this English PDF.");
            languageNotice.Format.Font.Size = 7;
            languageNotice.Format.Font.Color = Color.Parse("#8A6800");
            languageNotice.Format.SpaceAfter = Unit.FromMillimeter(5);
        }

        AddAthleteOverview(section, report);
        AddSummary(section, report.Summary);
        AddWeeklyProgress(section, report.WeeklyProgress);
        AddCheckIns(section, report.CheckIns, includeCoachNotes);

        if (includeCoachNotes && report.CoachNotes.Count > 0)
            AddCoachNotes(section, report.CoachNotes);

        if (photoSources.Count > 0)
            AddProgressPhotos(section, report.ProgressPhotos, photoSources);

        var generated = section.AddParagraph(
            $"Generated {DateTime.Parse(report.GeneratedAt).ToUniversalTime():dd MMM yyyy, HH:mm} UTC");
        generated.Format.SpaceBefore = Unit.FromMillimeter(6);
        generated.Format.Font.Size = 7;
        generated.Format.Font.Color = Color.Parse("#76767E");

        return document;
    }

    private static void AddHeader(Section section)
    {
        var header = section.Headers.Primary.AddTable();
        header.Borders.Bottom.Width = 0.75;
        header.Borders.Bottom.Color = Color.Parse("#FDC003");
        header.AddColumn(Unit.FromCentimeter(11.5));
        header.AddColumn(Unit.FromCentimeter(5));
        var row = header.AddRow();
        row.Cells[0].AddParagraph("JOKER NUTRITION").Format.Font.Bold = true;
        row.Cells[1].AddParagraph("COACHING PLATFORM").Format.Alignment = ParagraphAlignment.Right;
        row.Cells[1].Format.Font.Size = 7;
        row.Cells[1].Format.Font.Color = Color.Parse("#6B7280");
        row.BottomPadding = Unit.FromMillimeter(2);
    }

    private static void AddFooter(Section section)
    {
        var footer = section.Footers.Primary.AddParagraph();
        footer.Format.Alignment = ParagraphAlignment.Center;
        footer.Format.Font.Size = 7;
        footer.Format.Font.Color = Color.Parse("#76767E");
        footer.AddText("Sensitive coaching report  |  Page ");
        footer.AddPageField();
        footer.AddText(" of ");
        footer.AddNumPagesField();
    }

    private static void AddAthleteOverview(Section section, AthleteProgressReportDto report)
    {
        var table = section.AddTable();
        table.Borders.Width = 0.5;
        table.Borders.Color = Color.Parse("#D8DCE7");
        table.Shading.Color = Color.Parse("#F7F8FC");
        table.AddColumn(Unit.FromCentimeter(5.5));
        table.AddColumn(Unit.FromCentimeter(5.5));
        table.AddColumn(Unit.FromCentimeter(5.5));
        var row = table.AddRow();
        AddOverviewCell(
            row.Cells[0],
            "Goal",
            FormatPdfUserText(report.TargetGoal, "Not specified in English"));
        AddOverviewCell(row.Cells[1], "Height", report.HeightCm.HasValue ? $"{report.HeightCm:0.#} cm" : "No data");
        AddOverviewCell(row.Cells[2], "Current weight", FormatWeight(report.Summary.CurrentWeightKg));
    }

    private static void AddOverviewCell(Cell cell, string label, string value)
    {
        var labelParagraph = cell.AddParagraph(label.ToUpperInvariant());
        labelParagraph.Format.SpaceBefore = Unit.FromMillimeter(3);
        labelParagraph.Format.LeftIndent = Unit.FromMillimeter(3);
        labelParagraph.Format.Font.Size = 7;
        labelParagraph.Format.Font.Color = Color.Parse("#6B7280");
        var valueParagraph = cell.AddParagraph(value);
        valueParagraph.Format.SpaceAfter = Unit.FromMillimeter(3);
        valueParagraph.Format.LeftIndent = Unit.FromMillimeter(3);
        valueParagraph.Format.Font.Bold = true;
        valueParagraph.Format.Font.Size = 10;
    }

    private static void AddSummary(Section section, ProgressReportSummaryDto summary)
    {
        section.AddParagraph("Performance summary", StyleNames.Heading2);
        var table = section.AddTable();
        table.AddColumn(Unit.FromCentimeter(4.1));
        table.AddColumn(Unit.FromCentimeter(4.1));
        table.AddColumn(Unit.FromCentimeter(4.1));
        table.AddColumn(Unit.FromCentimeter(4.1));
        var row = table.AddRow();
        AddMetricCell(row.Cells[0], FormatSignedWeight(summary.WeightChangeKg), "Weight change");
        AddMetricCell(row.Cells[1], $"{summary.CompletedWorkouts}/{summary.LoggedWorkouts}", "Logged sessions completed");
        AddMetricCell(row.Cells[2], FormatPercent(summary.AverageCalorieAdherencePercent), "Calorie adherence");
        AddMetricCell(row.Cells[3], summary.CheckInCount.ToString(), "Check-ins submitted");
    }

    private static void AddMetricCell(Cell cell, string value, string label)
    {
        cell.Borders.Width = 0.5;
        cell.Borders.Color = Color.Parse("#D8DCE7");
        cell.Shading.Color = Color.Parse("#FFFFFF");
        var valueParagraph = cell.AddParagraph(value);
        valueParagraph.Format.SpaceBefore = Unit.FromMillimeter(3);
        valueParagraph.Format.LeftIndent = Unit.FromMillimeter(2.5);
        valueParagraph.Format.Font.Bold = true;
        valueParagraph.Format.Font.Size = 13;
        valueParagraph.Format.Font.Color = Color.Parse("#0B132B");
        var labelParagraph = cell.AddParagraph(label);
        labelParagraph.Format.SpaceAfter = Unit.FromMillimeter(3);
        labelParagraph.Format.LeftIndent = Unit.FromMillimeter(2.5);
        labelParagraph.Format.Font.Size = 7;
        labelParagraph.Format.Font.Color = Color.Parse("#6B7280");
    }

    private static void AddWeeklyProgress(Section section, IReadOnlyCollection<ProgressReportWeekDto> weeks)
    {
        section.AddParagraph("Weekly progress", StyleNames.Heading2);
        var table = section.AddTable();
        table.Borders.Width = 0.35;
        table.Borders.Color = Color.Parse("#D8DCE7");
        table.AddColumn(Unit.FromCentimeter(2.8));
        table.AddColumn(Unit.FromCentimeter(2.2));
        table.AddColumn(Unit.FromCentimeter(3));
        table.AddColumn(Unit.FromCentimeter(2.8));
        table.AddColumn(Unit.FromCentimeter(2.8));
        table.AddColumn(Unit.FromCentimeter(2.8));
        var header = table.AddRow();
        header.HeadingFormat = true;
        header.Shading.Color = Color.Parse("#0B132B");
        header.Format.Font.Color = Colors.White;
        header.Format.Font.Bold = true;
        AddTableText(header.Cells[0], "Week");
        AddTableText(header.Cells[1], "Weight");
        AddTableText(header.Cells[2], "Workouts");
        AddTableText(header.Cells[3], "Calories");
        AddTableText(header.Cells[4], "Protein");
        AddTableText(header.Cells[5], "Steps");

        foreach (var week in weeks.OrderByDescending(w => w.WeekOf))
        {
            var row = table.AddRow();
            row.TopPadding = Unit.FromMillimeter(1.7);
            row.BottomPadding = Unit.FromMillimeter(1.7);
            AddTableText(row.Cells[0], FormatDate(week.WeekOf));
            AddTableText(row.Cells[1], FormatWeight(week.WeightKg));
            AddTableText(row.Cells[2], week.LoggedWorkouts > 0
                ? $"{week.CompletedWorkouts}/{week.LoggedWorkouts} ({FormatPercent(week.WorkoutCompletionPercent)})"
                : "No logs");
            AddTableText(row.Cells[3], FormatPercent(week.CalorieAdherencePercent));
            AddTableText(row.Cells[4], FormatPercent(week.ProteinAdherencePercent));
            AddTableText(row.Cells[5], FormatPercent(week.StepsAdherencePercent));
        }
    }

    private static void AddCheckIns(
        Section section,
        IReadOnlyCollection<ProgressReportCheckInDto> checkIns,
        bool includeCoachNotes)
    {
        section.AddParagraph("Check-in history", StyleNames.Heading2);
        if (checkIns.Count == 0)
        {
            section.AddParagraph("No check-ins were submitted during this period.");
            return;
        }

        var table = section.AddTable();
        table.Borders.Width = 0.35;
        table.Borders.Color = Color.Parse("#D8DCE7");
        table.AddColumn(Unit.FromCentimeter(3.2));
        table.AddColumn(Unit.FromCentimeter(2.4));
        table.AddColumn(Unit.FromCentimeter(2.2));
        table.AddColumn(Unit.FromCentimeter(3.5));
        table.AddColumn(Unit.FromCentimeter(5.2));
        var header = table.AddRow();
        header.HeadingFormat = true;
        header.Shading.Color = Color.Parse("#E9EDFF");
        header.Format.Font.Bold = true;
        AddTableText(header.Cells[0], "Week");
        AddTableText(header.Cells[1], "Weight");
        AddTableText(header.Cells[2], "Waist");
        AddTableText(header.Cells[3], "Chest / thigh");
        AddTableText(header.Cells[4], "Wellbeing E / S / G / T");
        foreach (var checkIn in checkIns)
        {
            var row = table.AddRow();
            row.TopPadding = Unit.FromMillimeter(1.5);
            row.BottomPadding = Unit.FromMillimeter(1.5);
            AddTableText(row.Cells[0], FormatDate(checkIn.WeekOf));
            AddTableText(row.Cells[1], $"{checkIn.WeightKg:0.#} kg");
            AddTableText(row.Cells[2], checkIn.WaistCm.HasValue ? $"{checkIn.WaistCm:0.#} cm" : "-");
            AddTableText(row.Cells[3], $"{FormatCentimeters(checkIn.ChestCm)} / {FormatCentimeters(checkIn.ThighCm)}");
            AddTableText(row.Cells[4], $"{checkIn.EnergyLevel} / {checkIn.SleepQuality} / {checkIn.GutHealth} / {checkIn.TrainingStress}");
        }

        if (includeCoachNotes)
        {
            foreach (var checkIn in checkIns.Where(checkIn => !string.IsNullOrWhiteSpace(checkIn.ReviewNotes)))
            {
                var paragraph = section.AddParagraph();
                paragraph.Format.SpaceBefore = Unit.FromMillimeter(2);
                paragraph.Format.Borders.Left.Width = 2;
                paragraph.Format.Borders.Left.Color = Color.Parse("#FDC003");
                paragraph.Format.LeftIndent = Unit.FromMillimeter(3);
                paragraph.AddFormattedText($"{FormatDate(checkIn.WeekOf)} review: ", TextFormat.Bold);
                paragraph.AddText(FormatPdfUserText(checkIn.ReviewNotes, "Arabic review available in the coaching platform"));
            }
        }
    }

    private static void AddCoachNotes(Section section, IReadOnlyCollection<ProgressReportNoteDto> notes)
    {
        section.AddParagraph("Coach notes", StyleNames.Heading2);
        foreach (var note in notes)
        {
            var paragraph = section.AddParagraph();
            paragraph.Format.Borders.Left.Width = 2;
            paragraph.Format.Borders.Left.Color = Color.Parse("#FDC003");
            paragraph.Format.LeftIndent = Unit.FromMillimeter(3);
            paragraph.Format.SpaceAfter = Unit.FromMillimeter(3);
            paragraph.Format.KeepTogether = true;
            var heading = paragraph.AddFormattedText(
                $"{FormatPdfUserText(note.CoachName, "Coach")} - {DateTime.Parse(note.CreatedAt):dd MMM yyyy}\n");
            heading.Bold = true;
            paragraph.AddText(FormatPdfUserText(note.Text, "Arabic note available in the coaching platform"));
        }
    }

    private static void AddProgressPhotos(
        Section section,
        IReadOnlyCollection<ProgressReportPhotoDto> photos,
        IReadOnlyDictionary<int, string> photoSources)
    {
        section.AddPageBreak();
        section.AddParagraph("Progress photos", StyleNames.Heading2);
        section.AddParagraph("Earliest and latest photos from the selected reporting period.");
        var table = section.AddTable();
        table.AddColumn(Unit.FromCentimeter(5.3));
        table.AddColumn(Unit.FromCentimeter(5.3));
        table.AddColumn(Unit.FromCentimeter(5.3));

        var selectedPhotos = photos.Where(photo => photoSources.ContainsKey(photo.Id)).ToList();
        for (var index = 0; index < selectedPhotos.Count; index += 3)
        {
            var row = table.AddRow();
            for (var columnIndex = 0; columnIndex < 3 && index + columnIndex < selectedPhotos.Count; columnIndex++)
            {
                var photo = selectedPhotos[index + columnIndex];
                var cell = row.Cells[columnIndex];
                cell.Borders.Width = 0.5;
                cell.Borders.Color = Color.Parse("#D8DCE7");
                var image = cell.AddImage(photoSources[photo.Id]);
                image.LockAspectRatio = true;
                image.Width = Unit.FromCentimeter(4.8);
                var caption = cell.AddParagraph($"{photo.Angle} - {FormatDate(photo.WeekOf)}");
                caption.Format.Alignment = ParagraphAlignment.Center;
                caption.Format.Font.Size = 7;
                caption.Format.SpaceBefore = Unit.FromMillimeter(1);
                caption.Format.SpaceAfter = Unit.FromMillimeter(2);
            }
        }
    }

    private static void AddTableText(Cell cell, string text)
    {
        var paragraph = cell.AddParagraph(text);
        paragraph.Format.LeftIndent = Unit.FromMillimeter(1.5);
        paragraph.Format.RightIndent = Unit.FromMillimeter(1.5);
    }

    private async Task<Dictionary<int, string>> DownloadPhotosAsync(
        IReadOnlyCollection<ProgressReportPhotoDto> photos,
        CancellationToken cancellationToken)
    {
        var result = new System.Collections.Concurrent.ConcurrentDictionary<int, string>();
        var client = _httpClientFactory.CreateClient();
        client.Timeout = Timeout.InfiniteTimeSpan;
        using var totalTimeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        totalTimeout.CancelAfter(TimeSpan.FromSeconds(20));
        using var concurrency = new SemaphoreSlim(3);

        var tasks = photos.Take(6).Select(async photo =>
        {
            if (!Uri.TryCreate(photo.Url, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttps && !(uri.Scheme == Uri.UriSchemeHttp && uri.IsLoopback)) ||
                !_allowedPhotoHosts.Contains(uri.Host))
                return;

            var entered = false;
            try
            {
                await concurrency.WaitAsync(totalTimeout.Token);
                entered = true;
                using var requestTimeout = CancellationTokenSource.CreateLinkedTokenSource(totalTimeout.Token);
                requestTimeout.CancelAfter(TimeSpan.FromSeconds(8));
                using var response = await client.GetAsync(
                    uri,
                    HttpCompletionOption.ResponseHeadersRead,
                    requestTimeout.Token);
                response.EnsureSuccessStatusCode();
                if (response.Content.Headers.ContentLength > MaximumPhotoBytes)
                    return;
                var contentType = response.Content.Headers.ContentType?.MediaType;
                if (contentType is not ("image/jpeg" or "image/png"))
                    return;
                await using var content = await response.Content.ReadAsStreamAsync(requestTimeout.Token);
                var bytes = await ReadCappedAsync(content, MaximumPhotoBytes, requestTimeout.Token);
                if (bytes is not null && IsSafeDecodableImage(bytes))
                    result.TryAdd(photo.Id, $"base64:{Convert.ToBase64String(bytes)}");
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

    private static bool IsSafeDecodableImage(byte[] bytes)
    {
        try
        {
            using var stream = new MemoryStream(bytes, writable: false);
            using var image = XImage.FromStream(stream);
            return image.PixelWidth > 0 && image.PixelHeight > 0 &&
                   image.PixelWidth <= MaximumPhotoDimension && image.PixelHeight <= MaximumPhotoDimension &&
                   (long)image.PixelWidth * image.PixelHeight <= MaximumPhotoPixels;
        }
        catch
        {
            return false;
        }
    }

    private static string FormatDate(string value) =>
        DateOnly.TryParse(value, out var date) ? date.ToString("dd MMM yyyy") : value;

    private static string FormatWeight(decimal? value) => value.HasValue ? $"{value:0.#} kg" : "No data";

    private static string FormatCentimeters(decimal? value) => value.HasValue ? $"{value:0.#} cm" : "-";

    private static string FormatSignedWeight(decimal? value) =>
        value.HasValue ? $"{(value.Value > 0 ? "+" : string.Empty)}{value.Value:0.#} kg" : "No data";

    private static string FormatPercent(double? value) => value.HasValue ? $"{value:0.#}%" : "No data";

    private static string FormatPdfUserText(string? value, string fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
            return fallback;
        if (!value.Any(IsArabicCharacter))
            return value.Trim();

        var nonArabic = new string(value.Where(character => !IsArabicCharacter(character)).ToArray());
        var compact = string.Join(' ', nonArabic.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
            .Trim(' ', '/', '-', '–', '—', '.', ',', ':', ';');
        return compact.Any(char.IsLetterOrDigit) ? compact : fallback;
    }

    private static bool HasArabicContent(AthleteProgressReportDto report) =>
        ContainsArabic(report.AthleteName) ||
        ContainsArabic(report.TargetGoal) ||
        report.CheckIns.Any(checkIn => ContainsArabic(checkIn.ReviewNotes)) ||
        report.CoachNotes.Any(note => ContainsArabic(note.CoachName) || ContainsArabic(note.Text));

    private static bool ContainsArabic(string? value) =>
        !string.IsNullOrEmpty(value) && value.Any(IsArabicCharacter);

    private static bool IsArabicCharacter(char character) =>
        character is >= '\u0600' and <= '\u06FF' or
        >= '\u0750' and <= '\u077F' or
        >= '\u08A0' and <= '\u08FF' or
        >= '\uFB50' and <= '\uFDFF' or
        >= '\uFE70' and <= '\uFEFF';
}
