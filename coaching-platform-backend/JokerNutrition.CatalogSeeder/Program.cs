using System.Text.Json;
using JokerNutrition.CatalogSeeder;
using JokerNutrition.Data.Contexts;
using JokerNutrition.Data.Seeding;
using Microsoft.EntityFrameworkCore;
using Npgsql;

static string? ReadOption(string[] args, string name)
{
    var index = Array.FindIndex(args, arg => arg.Equals(name, StringComparison.OrdinalIgnoreCase));
    return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
}

static bool HasFlag(string[] args, string name) =>
    args.Any(arg => arg.Equals(name, StringComparison.OrdinalIgnoreCase));

static string? FirstConfigured(params string[] names) =>
    names.Select(Environment.GetEnvironmentVariable)
        .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));

static string NormalizeConnectionString(string raw)
{
    if (!raw.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) &&
        !raw.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        return raw;

    var uri = new Uri(raw);
    var userInfo = uri.UserInfo.Split(':', 2);
    if (userInfo.Length != 2)
        throw new InvalidOperationException("The PostgreSQL URL must contain a username and password.");

    return new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.IsDefaultPort ? 5432 : uri.Port,
        Database = uri.AbsolutePath.Trim('/'),
        Username = Uri.UnescapeDataString(userInfo[0]),
        Password = Uri.UnescapeDataString(userInfo[1]),
        SslMode = SslMode.Require,
        Pooling = true
    }.ConnectionString;
}

if (HasFlag(args, "--help") || HasFlag(args, "-h"))
{
    Console.WriteLine("""
        JokerNutrition production-safe catalog importer

        Required:
          --catalog <directory>       Directory containing manifest.json

        Optional:
          --apply                     Apply changes; without this flag the importer is dry-run only
          --sync-images               Resolve Azure URLs; uploads only when combined with --apply
          --coach-email <email>       Required when the package includes workout templates
          --applied-by <identifier>   Audit identifier (defaults to machine/user)
          --confirm-production <ver>  Required with --apply in Production; must equal catalog version

        Connection string environment variables (first non-empty wins):
          ConnectionStrings__DefaultConnection
          SUPABASE_DB_CONNECTION
          DATABASE_URL

        Azure Blob environment variables (required with --sync-images):
          BlobStorageSettings__ConnectionString (or AZURE_STORAGE_CONNECTION_STRING)
          BlobStorageSettings__ContainerName    (or AZURE_STORAGE_CONTAINER)
          BlobStorageSettings__PublicBaseUrl    (optional CDN/proxy URL for private containers)
        """);
    return 0;
}

var catalogDirectory = ReadOption(args, "--catalog")
    ?? throw new ArgumentException("--catalog <directory> is required.");
var apply = HasFlag(args, "--apply");
var syncImages = HasFlag(args, "--sync-images");
var coachEmail = ReadOption(args, "--coach-email");
var appliedBy = ReadOption(args, "--applied-by")
    ?? $"{Environment.UserName}@{Environment.MachineName}";

var package = await CoachCatalogPackage.LoadAsync(catalogDirectory);
if (apply && package.Recipes.Any(recipe => !string.IsNullOrWhiteSpace(recipe.ImageAssetPath)) && !syncImages)
    throw new InvalidOperationException(
        "This catalog contains recipe images. Production apply requires --sync-images so image URLs cannot be cleared or omitted.");
var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
    ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
    ?? "Production";

if (apply && environment.Equals("Production", StringComparison.OrdinalIgnoreCase))
{
    var confirmation = ReadOption(args, "--confirm-production");
    if (!package.Manifest.CatalogVersion.Equals(confirmation, StringComparison.Ordinal))
        throw new InvalidOperationException(
            $"Production apply requires --confirm-production {package.Manifest.CatalogVersion}.");
}

var rawConnection = FirstConfigured(
    "ConnectionStrings__DefaultConnection",
    "SUPABASE_DB_CONNECTION",
    "DATABASE_URL")
    ?? throw new InvalidOperationException("No database connection string environment variable is configured.");

var options = new DbContextOptionsBuilder<JokerNutritionContext>()
    // The importer owns one explicit all-or-nothing transaction. Enabling EF's
    // automatic retrying strategy here is incompatible with user transactions;
    // a failed run is safe to rerun because catalog upserts and blob names are
    // deterministic.
    .UseNpgsql(NormalizeConnectionString(rawConnection))
    .Options;

await using var context = new JokerNutritionContext(options);
if (!await context.Database.CanConnectAsync())
    throw new InvalidOperationException("The catalog importer could not connect to PostgreSQL.");

var pendingMigrations = (await context.Database.GetPendingMigrationsAsync()).ToList();
if (pendingMigrations.Count > 0)
    throw new InvalidOperationException(
        "Database migrations must be applied before catalog import: " + string.Join(", ", pendingMigrations));

var importer = new CatalogImporter(context);
if (apply)
{
    // Validate all database references and the coach account before mutating Azure.
    await importer.ImportAsync(package, true, coachEmail, appliedBy);
}

if (syncImages)
{
    var blobConnection = FirstConfigured(
        "BlobStorageSettings__ConnectionString",
        "AZURE_STORAGE_CONNECTION_STRING")
        ?? throw new InvalidOperationException("No Azure Blob connection string environment variable is configured.");
    var blobContainer = FirstConfigured(
        "BlobStorageSettings__ContainerName",
        "AZURE_STORAGE_CONTAINER")
        ?? throw new InvalidOperationException("No Azure Blob container name environment variable is configured.");
    var publicBaseUrl = Environment.GetEnvironmentVariable("BlobStorageSettings__PublicBaseUrl");

    var imageSynchronizer = new AzureCatalogImageSynchronizer(blobConnection, blobContainer, publicBaseUrl);
    var imageResult = await imageSynchronizer.SynchronizeAsync(package, catalogDirectory, !apply);
    Console.Error.WriteLine(
        $"Azure recipe images: {imageResult.ImageCount} files, {imageResult.TotalBytes} bytes, " +
        (imageResult.DryRun ? "dry-run only (no uploads)." : "uploaded with deterministic blob names."));
}

var result = await importer.ImportAsync(package, !apply, coachEmail, appliedBy);
Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
return 0;
