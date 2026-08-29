using Microsoft.AspNetCore.Identity;

namespace ModernBlog.Core.Identity
{
    public sealed class ApplicationUser : IdentityUser
    {
        public string? DisplayName { get; set; }
    }
}
