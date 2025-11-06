using Mailjet.Client;
using Mailjet.Client.TransactionalEmails;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TiroTime.Application.Common;
using TiroTime.Application.DTOs;
using TiroTime.Application.Interfaces;

namespace TiroTime.Infrastructure.Services;

public class EmailService(
    IConfiguration configuration,
    ILogger<EmailService> logger) : IEmailService
{
    public async Task<Result<bool>> SendEmailWithAttachmentsAsync(SendEmailDto dto)
    {
        try
        {
            var apiKey = configuration["Mailjet:ApiKey"];
            var apiSecret = configuration["Mailjet:ApiSecret"];
            var fromEmail = configuration["Mailjet:FromEmail"];
            var fromName = configuration["Mailjet:FromName"];

            if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
            {
                logger.LogError("Mailjet API credentials are not configured");
                return Result.Failure<bool>("E-Mail-Dienst ist nicht konfiguriert.");
            }

            if (string.IsNullOrEmpty(fromEmail) || string.IsNullOrEmpty(fromName))
            {
                logger.LogError("Mailjet sender information is not configured");
                return Result.Failure<bool>("E-Mail-Absender ist nicht konfiguriert.");
            }

            var client = new MailjetClient(apiKey, apiSecret);

            // Build the email message
            var email = new TransactionalEmailBuilder()
                .WithFrom(new SendContact(fromEmail, fromName))
                .WithTo(new SendContact(dto.ToEmail, dto.ToName))
                .WithSubject(dto.Subject)
                .WithTextPart(dto.TextBody)
                .WithHtmlPart(dto.HtmlBody);

            // Add attachments
            foreach (var attachment in dto.Attachments)
            {
                var base64Content = Convert.ToBase64String(attachment.Content);
                var mailjetAttachment = new Attachment(attachment.FileName, attachment.ContentType, base64Content);
                email.WithAttachment(mailjetAttachment);
            }

            var builtEmail = email.Build();

            // Send the email
            var response = await client.SendTransactionalEmailAsync(builtEmail);

            if (response.Messages.Length > 0 && response.Messages[0].Status == "success")
            {
                logger.LogInformation("Email sent successfully to {ToEmail}", dto.ToEmail);
                return Result.Success(true);
            }

            logger.LogError("Failed to send email. Status: {Status}", response.Messages.Length > 0 ? response.Messages[0].Status : "Unknown");
            return Result.Failure<bool>("E-Mail konnte nicht gesendet werden.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending email to {ToEmail}", dto.ToEmail);
            return Result.Failure<bool>($"Fehler beim Senden der E-Mail: {ex.Message}");
        }
    }
}
