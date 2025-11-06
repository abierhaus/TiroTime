namespace TiroTime.Application.DTOs;

public record SendEmailDto(
    string ToEmail,
    string ToName,
    string Subject,
    string TextBody,
    string HtmlBody,
    List<EmailAttachmentDto> Attachments);

public record EmailAttachmentDto(
    string FileName,
    string ContentType,
    byte[] Content);
