using Mailjet.Client;
using Mailjet.Client.TransactionalEmails;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TiroTime.Application.Common;
using TiroTime.Application.DTOs;
using TiroTime.Application.Interfaces;
using TiroTime.Infrastructure.Options;

namespace TiroTime.Infrastructure.Services;

/// <summary>
/// E-Mail-Versand über Mailjet. Der <see cref="HttpClient"/> kommt aus der IHttpClientFactory
/// (typed client), statt pro Versand einen neuen MailjetClient mit eigenem HttpClient zu erzeugen.
/// </summary>
public sealed class EmailService(
    HttpClient httpClient,
    IOptionsSnapshot<MailjetOptions> options,
    ILogger<EmailService> logger) : IEmailService
{
    public async Task<Result<bool>> SendEmailWithAttachmentsAsync(SendEmailDto dto)
    {
        try
        {
            var settings = options.Value;

            if (!settings.HasCredentials)
            {
                logger.LogError("Mailjet API credentials are not configured");
                return Result.Failure<bool>("E-Mail-Dienst ist nicht konfiguriert.");
            }

            if (!settings.HasSender)
            {
                logger.LogError("Mailjet sender information is not configured");
                return Result.Failure<bool>("E-Mail-Absender ist nicht konfiguriert.");
            }

            httpClient.SetDefaultSettings();
            httpClient.UseBasicAuthentication(settings.ApiKey!, settings.ApiSecret!);
            var client = new MailjetClient(httpClient);

            var email = new TransactionalEmailBuilder()
                .WithFrom(new SendContact(settings.FromEmail!, settings.FromName!))
                .WithTo(new SendContact(dto.ToEmail, dto.ToName))
                .WithSubject(dto.Subject)
                .WithTextPart(dto.TextBody)
                .WithHtmlPart(dto.HtmlBody);

            foreach (var attachment in dto.Attachments)
            {
                var base64Content = Convert.ToBase64String(attachment.Content);
                email.WithAttachment(new Attachment(attachment.FileName, attachment.ContentType, base64Content));
            }

            var response = await client.SendTransactionalEmailAsync(email.Build());

            if (response.Messages.Length > 0 && response.Messages[0].Status == "success")
            {
                logger.LogInformation("Email sent successfully to {ToEmail}", dto.ToEmail);
                return Result.Success(true);
            }

            logger.LogError("Failed to send email. Status: {Status}",
                response.Messages.Length > 0 ? response.Messages[0].Status : "Unknown");
            return Result.Failure<bool>("E-Mail konnte nicht gesendet werden.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending email to {ToEmail}", dto.ToEmail);
            return Result.Failure<bool>($"Fehler beim Senden der E-Mail: {ex.Message}");
        }
    }
}
