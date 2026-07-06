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
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public bool UseSsl { get; set; } = true;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "Joker Nutrition";
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
