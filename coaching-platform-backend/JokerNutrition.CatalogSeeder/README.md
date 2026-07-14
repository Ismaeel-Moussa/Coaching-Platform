# Coach Catalog Importer

This project imports versioned coach content without creating demo users, assigning
content to athletes, or overwriting athlete-created records.

Dry run is the default:

```powershell
$env:ConnectionStrings__DefaultConnection = '<server-side PostgreSQL connection string>'
dotnet run --project JokerNutrition.CatalogSeeder -- `
  --catalog '<private-catalog-directory>' `
  --coach-email coach@example.com `
  --sync-images
```

Production apply requires both `--apply` and an exact catalog-version confirmation:

```powershell
$env:DOTNET_ENVIRONMENT = 'Production'
$env:SUPABASE_DB_CONNECTION = '<Supabase direct or session-pooler PostgreSQL connection string>'
$env:BlobStorageSettings__ConnectionString = '<existing Azure Blob connection string>'
$env:BlobStorageSettings__ContainerName = '<existing public container>'
dotnet run --project JokerNutrition.CatalogSeeder -- `
  --catalog '<private-catalog-directory>' `
  --coach-email coach@example.com `
  --sync-images `
  --apply `
  --confirm-production '<exact-version-from-manifest>' `
  --applied-by release-operator@example.com
```

Apply migrations, back up the database, and review the dry-run JSON before an apply.
Private catalog packages are intentionally excluded from this public repository.
Draft and InReview records must not be published until their required coach, nutrition,
and safety reviews are complete.

## Recipe images

Approved recipe images belong inside the private catalog package. Their checksums must
be included in `manifest.json`, and recipes reference them through `imageAssetPath`.

`--sync-images` validates the existing Azure container and resolves deterministic
URLs during a dry run. It uploads only when `--apply` is also supplied. Blob names
follow `coach-catalog/recipes/{seedKey}.v{contentVersion}.webp`, so a retry overwrites
the same version instead of creating duplicates. The catalog importer intentionally
does not use the API upload service's local-disk fallback: an Azure failure stops the
production import.

If the container is private, configure `BlobStorageSettings__PublicBaseUrl` with the
public CDN or image-proxy base URL. The importer will otherwise stop, because a raw
private blob URL would not render in the web application.
