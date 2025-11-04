using Microsoft.AspNetCore.Identity;

namespace TiroTime.Domain.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
    public string LanguageCode { get; set; } = "de";
    public string TimeZone { get; set; } = "Europe/Berlin";
    public UserStatus Status { get; set; } = UserStatus.Active;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }

    public ApplicationUser()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }

    public string FullName => $"{FirstName} {LastName}".Trim();
}
