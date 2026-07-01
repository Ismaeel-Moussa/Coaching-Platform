using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using JokerNutrition.Business.Configurations;

namespace JokerNutrition.Business.Services;

public interface IEmailService
{
    Task SendInvitationEmailAsync(string toEmail, string inviteUrl, string role);
    Task SendPasswordResetEmailAsync(string toEmail, string resetUrl);
}

/// <summary>
/// Email service — sends real emails via SMTP using MailKit.
/// </summary>
public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly SmtpSettings _smtpSettings;

    public EmailService(ILogger<EmailService> logger, IOptions<SmtpSettings> smtpSettings)
    {
        _logger = logger;
        _smtpSettings = smtpSettings.Value;
    }

    public async Task SendInvitationEmailAsync(string toEmail, string inviteUrl, string role)
    {
        _logger.LogInformation("Sending invitation email to {Email} as {Role} via SMTP", toEmail, role);

        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_smtpSettings.FromName, _smtpSettings.FromEmail));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = "Welcome to Joker Nutrition Coaching Platform — Your Invitation Link";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
                    <h2>Welcome to Joker Nutrition Coaching Platform!</h2>
                    <p>You have been invited to join the team as an <strong>{role}</strong>.</p>
                    <p>To register and setup your account, please click the link below:</p>
                    <p><a href='{inviteUrl}' style='padding: 10px 20px; background-color: #fdc003; color: #0b132b; text-decoration: none; border-radius: 4px; font-weight: bold;'>Join the Team</a></p>
                    <p>Or copy and paste this link into your browser:</p>
                    <p>{inviteUrl}</p>
                    <br />
                    <p>Best regards,<br />The Joker Nutrition Team</p>"
            };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(_smtpSettings.Host, _smtpSettings.Port, MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

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
        _logger.LogInformation("Sending password reset email to {Email} via SMTP", toEmail);

        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_smtpSettings.FromName, _smtpSettings.FromEmail));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = "Reset Your Joker Nutrition Password";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
                    <h2>Reset Your Password</h2>
                    <p>We received a request to reset the password for your Joker Nutrition account.</p>
                    <p>Click the link below to set a new password:</p>
                    <p><a href='{resetUrl}' style='padding: 10px 20px; background-color: #ba1a1a; color: #ffffff; text-decoration: none; border-radius: 4px; font-weight: bold;'>Reset Password</a></p>
                    <p>Or copy and paste this link into your browser:</p>
                    <p>{resetUrl}</p>
                    <br />
                    <p>Best regards,<br />The Joker Nutrition Team</p>"
            };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(_smtpSettings.Host, _smtpSettings.Port, MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Password reset email sent successfully to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password reset email to {Email}", toEmail);
            throw;
        }
    }
}

