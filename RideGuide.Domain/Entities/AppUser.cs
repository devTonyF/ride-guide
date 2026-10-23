using Microsoft.AspNetCore.Identity;

namespace RideGuide.Domain.Entities;

public class AppUser : IdentityUser<int>
{
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}
