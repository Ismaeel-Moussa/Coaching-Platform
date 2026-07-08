using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Resend;
using JokerNutrition.Business.Configurations;

namespace JokerNutrition.Business.Services;

public interface IEmailService
{
    Task SendInvitationEmailAsync(string toEmail, string inviteUrl, string role);
    Task SendPasswordResetEmailAsync(string toEmail, string resetUrl);
}

/// <summary>
/// Email service — sends emails via Resend HTTP API (required for Render hosting,
/// which blocks outbound SMTP connections on ports 587/465).
/// </summary>
public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly SmtpSettings _settings;
    private readonly IResend _resend;

    public EmailService(ILogger<EmailService> logger, IOptions<SmtpSettings> settings, IResend resend)
    {
        _logger = logger;
        _settings = settings.Value;
        _resend = resend;
    }

    public async Task SendInvitationEmailAsync(string toEmail, string inviteUrl, string role)
    {
        _logger.LogInformation("Sending invitation email to {Email} as {Role} via Resend", toEmail, role);

        try
        {
            var message = new EmailMessage
            {
                From = $"{_settings.FromName} <{_settings.FromEmail}>",
                Subject = "Welcome to Joker Nutrition Coaching Platform — Your Invitation Link",
                HtmlBody = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                        <h2 style='color: #0b132b;'>Welcome to Joker Nutrition Coaching Platform!</h2>
                        <p>You have been invited to join the team as an <strong>{role}</strong>.</p>
                        <p>To register and set up your account, please click the link below:</p>
                        <p>
                            <a href='{inviteUrl}' style='display:inline-block; padding: 12px 24px; background-color: #fdc003; color: #0b132b; text-decoration: none; border-radius: 4px; font-weight: bold;'>
                                Join the Team
                            </a>
                        </p>
                        <p style='color:#666;'>Or copy and paste this link into your browser:</p>
                        <p style='word-break:break-all;'>{inviteUrl}</p>
                        <br />
                        <p>Best regards,<br />The Joker Nutrition Team</p>
                    </div>"
            };
            message.To.Add(toEmail);

            await _resend.EmailSendAsync(message);

            _logger.LogInformation("Invitation email sent successfully to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send invitation email to {Email}", toEmail);
            throw;
        }
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string resetUrl)
    {
        _logger.LogInformation("Sending password reset email to {Email} via Resend", toEmail);

        try
        {
            var message = new EmailMessage
            {
                From = $"{_settings.FromName} <{_settings.FromEmail}>",
                Subject = "Reset Your Joker Nutrition Password",
                HtmlBody = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                        <h2 style='color: #0b132b;'>Reset Your Password</h2>
                        <p>We received a request to reset the password for your Joker Nutrition account.</p>
                        <p>Click the link below to set a new password:</p>
                        <p>
                            <a href='{resetUrl}' style='display:inline-block; padding: 12px 24px; background-color: #ba1a1a; color: #ffffff; text-decoration: none; border-radius: 4px; font-weight: bold;'>
                                Reset Password
                            </a>
                        </p>
                        <p style='color:#666;'>Or copy and paste this link into your browser:</p>
                        <p style='word-break:break-all;'>{resetUrl}</p>
                        <br />
                        <p>Best regards,<br />The Joker Nutrition Team</p>
                    </div>"
            };
            message.To.Add(toEmail);

            await _resend.EmailSendAsync(message);

            _logger.LogInformation("Password reset email sent successfully to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password reset email to {Email}", toEmail);
            throw;
        }
    }
}
