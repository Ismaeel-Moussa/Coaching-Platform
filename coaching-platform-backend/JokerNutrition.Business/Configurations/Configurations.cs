namespace JokerNutrition.Business.Configurations;

public class JwtSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpiryInDays { get; set; } = 1;
}

public class SmtpSettings
{
    // Resend HTTP API (replaces direct SMTP — required for Render hosting)
    public string ResendApiKey { get; set; } = string.Empty;

    // Sender identity
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "Joker Nutrition";

    // URLs
    public string SignUpBaseUrl { get; set; } = string.Empty;
    public string SupportEmail { get; set; } = string.Empty;
}

public class BlobStorageSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string ContainerName { get; set; } = string.Empty;
    public string LocalFallbackBaseUrl { get; set; } = string.Empty;
}

public class AppSettings
{
    public string ResetPasswordPageUrl { get; set; } = string.Empty;
}
