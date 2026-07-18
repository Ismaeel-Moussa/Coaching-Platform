using System.Reflection;
using PdfSharp.Fonts;

namespace JokerNutrition.Business.Reports;

public sealed class ProgressReportFontResolver : IFontResolver
{
    public const string FamilyName = "DejaVu Sans Embedded";
    private const string RegularFace = "DejaVuSans-Regular";
    private const string BoldFace = "DejaVuSans-Bold";
    private static readonly object SyncRoot = new();
    private static readonly ProgressReportFontResolver Instance = new();

    public static void EnsureConfigured()
    {
        if (GlobalFontSettings.FontResolver is ProgressReportFontResolver)
            return;

        lock (SyncRoot)
        {
            if (GlobalFontSettings.FontResolver is null)
                GlobalFontSettings.FontResolver = Instance;
        }
    }

    public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic) =>
        new(isBold ? BoldFace : RegularFace);

    public byte[] GetFont(string faceName)
    {
        var resourceName = faceName == BoldFace
            ? "JokerNutrition.Business.Assets.Fonts.DejaVuSans-Bold.ttf"
            : "JokerNutrition.Business.Assets.Fonts.DejaVuSans.ttf";
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded PDF font '{resourceName}' was not found.");
        using var memory = new MemoryStream();
        stream.CopyTo(memory);
        return memory.ToArray();
    }
}
