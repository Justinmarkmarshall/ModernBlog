using Microsoft.AspNetCore.Identity;
using ModernBlog.Web.Data;

namespace ModernBlog.Web.Components.Account;

// Email verification is independent of administrator approval.
public sealed class AdministratorApprovalConfirmation : IUserConfirmation<ApplicationUser>
{
    public Task<bool> IsConfirmedAsync(UserManager<ApplicationUser> manager, ApplicationUser user)
        => Task.FromResult(user.ApprovalStatus == AccountApprovalStatus.Approved);
}
