using TiroTime.Application.Common;
using TiroTime.Application.DTOs;

namespace TiroTime.Application.Interfaces;

public interface IEmailService
{
    Task<Result<bool>> SendEmailWithAttachmentsAsync(SendEmailDto dto);
}
