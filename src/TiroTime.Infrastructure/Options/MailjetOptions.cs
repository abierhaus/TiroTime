namespace TiroTime.Infrastructure.Options;

/// <summary>
/// Konfiguration für den Mailjet-Versand (Abschnitt "Mailjet"; Secrets kommen aus User Secrets bzw. Umgebungsvariablen Mailjet__*).
/// </summary>
public sealed class MailjetOptions
{
    public const string SectionName = "Mailjet";

    public string? ApiKey { get; set; }
    public string? ApiSecret { get; set; }
    public string? FromEmail { get; set; }
    public string? FromName { get; set; } = "TiroTime";

    public bool HasCredentials => !string.IsNullOrEmpty(ApiKey) && !string.IsNullOrEmpty(ApiSecret);
    public bool HasSender => !string.IsNullOrEmpty(FromEmail) && !string.IsNullOrEmpty(FromName);
}
