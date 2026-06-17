using Microsoft.Extensions.Logging;

namespace JokerNutrition.Business.Services;

public interface IEmailService
{
    Task SendInvitationEmailAsync(string toEmail, string inviteUrl, string role);
    Task SendPasswordResetEmailAsync(string toEmail, string resetUrl);
}

/// <summary>
/// Mocked email service — logs to Serilog instead of sending real emails.
/// Replace with MailKit implementation when SMTP is configured.
/// </summary>
public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public Task SendInvitationEmailAsync(string toEmail, string inviteUrl, string role)
    {
        _logger.LogWarning(
            "[MOCK EMAIL] Invitation to {Email} as {Role}. Join link: {Url}",
            toEmail, role, inviteUrl);
        return Task.CompletedTask;
    }

    public Task SendPasswordResetEmailAsync(string toEmail, string resetUrl)
    {
        _logger.LogWarning(
            "[MOCK EMAIL] Password reset for {Email}. Reset link: {Url}",
            toEmail, resetUrl);
        return Task.CompletedTask;
    }
}
