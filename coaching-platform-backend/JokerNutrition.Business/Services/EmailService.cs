using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Resend;
using JokerNutrition.Business.Configurations;

namespace JokerNutrition.Business.Services;

public interface IEmailService
{
    Task SendInvitationEmailAsync(string toEmail, string inviteUrl, string role);
    Task SendPasswordResetEmailAsync(string toEmail, string resetUrl);
    Task SendAccountStatusEmailAsync(string toEmail, string userName, bool isActive, string? reason = null);
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
                    <div style='background-color: #f8fafc; padding: 40px 20px; font-family: ""Outfit"", ""Inter"", -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, sans-serif;'>
                        <div style='max-width: 540px; margin: 0 auto; background-color: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 10px 25px -5px rgba(0, 0, 0, 0.05), 0 8px 10px -6px rgba(0, 0, 0, 0.05); border: 1px solid #e2e8f0;'>
                            <!-- Header -->
                            <div style='background-color: #0b132b; padding: 32px; text-align: center;'>
                                <div style='color: #fdc003; font-size: 24px; font-weight: 800; letter-spacing: 2px; text-transform: uppercase;'>
                                    Joker Nutrition
                                </div>
                                <div style='color: #94a3b8; font-size: 13px; margin-top: 4px; letter-spacing: 1px;'>
                                    COACHING PLATFORM
                                </div>
                            </div>
                            
                            <div style='padding: 40px 32px 32px 32px;'>
                                <h2 style='color: #0f172a; font-size: 22px; font-weight: 700; margin: 0 0 16px 0; text-align: center;'>
                                    You're Invited!
                                </h2>
                                
                                <p style='color: #475569; font-size: 15px; line-height: 24px; margin: 0 0 24px 0; text-align: center;'>
                                    You have been invited to join the Joker Nutrition Coaching Platform as an <strong style='color: #0b132b; background-color: #fef08a; padding: 2px 8px; border-radius: 4px; font-size: 14px;'>{role}</strong>.
                                </p>
                                
                                <div style='text-align: center; margin-bottom: 32px;'>
                                    <a href='{inviteUrl}' style='display: inline-block; background-color: #fdc003; color: #0b132b; text-decoration: none; font-size: 16px; font-weight: 700; padding: 14px 32px; border-radius: 8px; box-shadow: 0 4px 6px -1px rgba(253, 192, 3, 0.2), 0 2px 4px -2px rgba(253, 192, 3, 0.2);'>
                                        Join the Team
                                    </a>
                                </div>
                                
                                <div style='border-top: 1px dashed #e2e8f0; margin-top: 32px; padding-top: 24px;'>
                                    <p style='color: #64748b; font-size: 13px; line-height: 20px; margin: 0 0 8px 0; text-align: center;'>
                                        If the button above doesn't work, copy and paste this link into your browser:
                                    </p>
                                    <div style='background-color: #f8fafc; border: 1px solid #e2e8f0; border-radius: 8px; padding: 12px; word-break: break-all; font-family: monospace; font-size: 12px; color: #334155; text-align: center;'>
                                        {inviteUrl}
                                    </div>
                                </div>
                            </div>
                            
                            <!-- Footer -->
                            <div style='background-color: #f8fafc; padding: 24px; text-align: center; border-top: 1px solid #e2e8f0;'>
                                <p style='color: #64748b; font-size: 13px; margin: 0 0 8px 0;'>
                                    Need help? Contact us at <a href='mailto:{_settings.SupportEmail}' style='color: #fdc003; text-decoration: none; font-weight: 600;'>{_settings.SupportEmail}</a>
                                </p>
                                <p style='color: #94a3b8; font-size: 11px; margin: 0;'>
                                    &copy; {DateTime.UtcNow.Year} Joker Nutrition. All rights reserved.
                                </p>
                            </div>
                        </div>
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
                    <div style='background-color: #f8fafc; padding: 40px 20px; font-family: ""Outfit"", ""Inter"", -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, sans-serif;'>
                        <div style='max-width: 540px; margin: 0 auto; background-color: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 10px 25px -5px rgba(0, 0, 0, 0.05), 0 8px 10px -6px rgba(0, 0, 0, 0.05); border: 1px solid #e2e8f0;'>
                            <!-- Header -->
                            <div style='background-color: #0b132b; padding: 32px; text-align: center;'>
                                <div style='color: #fdc003; font-size: 24px; font-weight: 800; letter-spacing: 2px; text-transform: uppercase;'>
                                    Joker Nutrition
                                </div>
                                <div style='color: #94a3b8; font-size: 13px; margin-top: 4px; letter-spacing: 1px;'>
                                    COACHING PLATFORM
                                </div>
                            </div>
                            
                            <div style='padding: 40px 32px 32px 32px;'>
                                <h2 style='color: #0f172a; font-size: 22px; font-weight: 700; margin: 0 0 16px 0; text-align: center;'>
                                    Reset Your Password
                                </h2>
                                
                                <p style='color: #475569; font-size: 15px; line-height: 24px; margin: 0 0 24px 0; text-align: center;'>
                                    We received a request to reset the password for your Joker Nutrition account. If you did not make this request, you can safely ignore this email.
                                </p>
                                
                                <div style='text-align: center; margin-bottom: 32px;'>
                                    <a href='{resetUrl}' style='display: inline-block; background-color: #ba1a1a; color: #ffffff; text-decoration: none; font-size: 16px; font-weight: 700; padding: 14px 32px; border-radius: 8px; box-shadow: 0 4px 6px -1px rgba(186, 26, 26, 0.2), 0 2px 4px -2px rgba(186, 26, 26, 0.2);'>
                                        Reset Password
                                    </a>
                                </div>
                                
                                <div style='border-top: 1px dashed #e2e8f0; margin-top: 32px; padding-top: 24px;'>
                                    <p style='color: #64748b; font-size: 13px; line-height: 20px; margin: 0 0 8px 0; text-align: center;'>
                                        If the button above doesn't work, copy and paste this link into your browser:
                                    </p>
                                    <div style='background-color: #f8fafc; border: 1px solid #e2e8f0; border-radius: 8px; padding: 12px; word-break: break-all; font-family: monospace; font-size: 12px; color: #334155; text-align: center;'>
                                        {resetUrl}
                                    </div>
                                </div>
                            </div>
                            
                            <!-- Footer -->
                            <div style='background-color: #f8fafc; padding: 24px; text-align: center; border-top: 1px solid #e2e8f0;'>
                                <p style='color: #64748b; font-size: 13px; margin: 0 0 8px 0;'>
                                    Need help? Contact us at <a href='mailto:{_settings.SupportEmail}' style='color: #fdc003; text-decoration: none; font-weight: 600;'>{_settings.SupportEmail}</a>
                                </p>
                                <p style='color: #94a3b8; font-size: 11px; margin: 0;'>
                                    &copy; {DateTime.UtcNow.Year} Joker Nutrition. All rights reserved.
                                </p>
                            </div>
                        </div>
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

    public async Task SendAccountStatusEmailAsync(string toEmail, string userName, bool isActive, string? reason = null)
    {
        var actionText = isActive ? "Reactivated" : "Suspended";
        _logger.LogInformation("Sending account {Action} email to {Email} via Resend", actionText.ToLowerInvariant(), toEmail);

        try
        {
            var subject = isActive
                ? "Account Reactivated — Joker Nutrition"
                : "Account Suspended — Joker Nutrition";

            string bodyContent;
            if (isActive)
            {
                bodyContent = $@"
                    <h2 style='color: #0f172a; font-size: 22px; font-weight: 700; margin: 0 0 16px 0; text-align: center;'>
                        Account Reactivated
                    </h2>
                    
                    <p style='color: #475569; font-size: 15px; line-height: 24px; margin: 0 0 20px 0; text-align: center;'>
                        Hello <strong>{userName}</strong>, great news! Your account on the <strong>Joker Nutrition Coaching Platform</strong> has been reactivated.
                    </p>
                    
                    <div style='background-color: #f0fdf4; border: 1px solid #bbf7d0; border-radius: 8px; padding: 16px; margin-bottom: 24px; text-align: center; color: #166534; font-size: 14px; font-weight: 600;'>
                        Your access to the platform has been fully restored.
                    </div>
                    
                    <p style='color: #475569; font-size: 14px; line-height: 22px; margin: 0 0 24px 0; text-align: center;'>
                        You can now log back into your account and continue your coaching program.
                    </p>
                    
                    <div style='text-align: center; margin-top: 28px; margin-bottom: 12px;'>
                        <a href='https://jokernutrition.app/login' style='display: inline-block; background-color: #fdc003; color: #0b132b; text-decoration: none; font-size: 16px; font-weight: 700; padding: 14px 32px; border-radius: 8px; box-shadow: 0 4px 6px -1px rgba(253, 192, 3, 0.2), 0 2px 4px -2px rgba(253, 192, 3, 0.2);'>
                            Log In to Platform
                        </a>
                    </div>";
            }
            else
            {
                var reasonMarkup = !string.IsNullOrWhiteSpace(reason)
                    ? $@"
                    <div style='background-color: #fef2f2; border: 1px solid #fecaca; border-radius: 8px; padding: 16px; margin: 20px 0; text-align: left;'>
                        <div style='color: #991b1b; font-weight: 700; font-size: 13px; text-transform: uppercase; letter-spacing: 0.5px; margin-bottom: 6px;'>Reason for Suspension:</div>
                        <div style='color: #7f1d1d; font-size: 14px; line-height: 20px; font-weight: 500;'>{reason}</div>
                    </div>"
                    : string.Empty;

                bodyContent = $@"
                    <h2 style='color: #991b1b; font-size: 22px; font-weight: 700; margin: 0 0 16px 0; text-align: center;'>
                        Account Suspended
                    </h2>
                    
                    <p style='color: #475569; font-size: 15px; line-height: 24px; margin: 0 0 16px 0; text-align: center;'>
                        Hello <strong>{userName}</strong>, your account on the <strong>Joker Nutrition Coaching Platform</strong> has been suspended by an administrator.
                    </p>
                    
                    {reasonMarkup}
                    
                    <p style='color: #64748b; font-size: 14px; line-height: 22px; margin: 20px 0 0 0; text-align: center;'>
                        If you believe this is an error or have questions regarding your account status, please contact our support team.
                    </p>";
            }

            var message = new EmailMessage
            {
                From = $"{_settings.FromName} <{_settings.FromEmail}>",
                Subject = subject,
                HtmlBody = $@"
                    <div style='background-color: #f8fafc; padding: 40px 20px; font-family: ""Outfit"", ""Inter"", -apple-system, BlinkMacSystemFont, ""Segoe UI"", Roboto, sans-serif;'>
                        <div style='max-width: 540px; margin: 0 auto; background-color: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 10px 25px -5px rgba(0, 0, 0, 0.05), 0 8px 10px -6px rgba(0, 0, 0, 0.05); border: 1px solid #e2e8f0;'>
                            <!-- Header -->
                            <div style='background-color: #0b132b; padding: 32px; text-align: center;'>
                                <div style='color: #fdc003; font-size: 24px; font-weight: 800; letter-spacing: 2px; text-transform: uppercase;'>
                                    Joker Nutrition
                                </div>
                                <div style='color: #94a3b8; font-size: 13px; margin-top: 4px; letter-spacing: 1px;'>
                                    COACHING PLATFORM
                                </div>
                            </div>
                            
                            <div style='padding: 40px 32px 32px 32px;'>
                                {bodyContent}
                            </div>
                            
                            <!-- Footer -->
                            <div style='background-color: #f8fafc; padding: 24px; text-align: center; border-top: 1px solid #e2e8f0;'>
                                <p style='color: #64748b; font-size: 13px; margin: 0 0 8px 0;'>
                                    Need help? Contact us at <a href='mailto:{_settings.SupportEmail}' style='color: #fdc003; text-decoration: none; font-weight: 600;'>{_settings.SupportEmail}</a>
                                </p>
                                <p style='color: #94a3b8; font-size: 11px; margin: 0;'>
                                    &copy; {DateTime.UtcNow.Year} Joker Nutrition. All rights reserved.
                                </p>
                            </div>
                        </div>
                    </div>"
            };
            message.To.Add(toEmail);

            await _resend.EmailSendAsync(message);

            _logger.LogInformation("Account status ({Action}) email sent successfully to {Email}", actionText, toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send account status email to {Email}", toEmail);
            throw;
        }
    }
}

