using Microsoft.AspNetCore.Identity;

namespace ModernBlog.Web.Data;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    public AccountApprovalStatus ApprovalStatus { get; set; } = AccountApprovalStatus.Pending;
}

public enum AccountApprovalStatus
{
    Pending,
    Approved,
    Rejected
}

