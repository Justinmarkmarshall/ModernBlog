using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using ModernBlog.Web.Components.Account;
using ModernBlog.Web.Components.Pages.Admin;
using ModernBlog.Web.Data;

namespace ModernBlog.IntegrationTests;

public class AccountApprovalTests
{
    [Theory]
    [InlineData(AccountApprovalStatus.Pending)]
    [InlineData(AccountApprovalStatus.Rejected)]
    public async Task UnapprovedUsersCannotSignInEvenWithVerifiedEmail(AccountApprovalStatus status)
    {
        await using var provider = CreateServices();
        await using var scope = provider.CreateAsyncScope();
        await InitializeAsync(scope.ServiceProvider);
        var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var signIn = scope.ServiceProvider.GetRequiredService<SignInManager<ApplicationUser>>();
        var user = new ApplicationUser { UserName = "applicant", Email = "applicant@example.com", EmailConfirmed = true, ApprovalStatus = status };
        Assert.True((await manager.CreateAsync(user, "ValidPassword123!")).Succeeded);
        Assert.True((await manager.AddLoginAsync(user, new("Google", "google-user-id", "Google"))).Succeeded);

        Assert.True((await signIn.CheckPasswordSignInAsync(user, "ValidPassword123!", false)).IsNotAllowed);
        Assert.True((await signIn.ExternalLoginSignInAsync("Google", "google-user-id", false)).IsNotAllowed);

        user.ApprovalStatus = AccountApprovalStatus.Approved;
        Assert.True((await manager.UpdateAsync(user)).Succeeded);
        Assert.True((await signIn.CheckPasswordSignInAsync(user, "ValidPassword123!", false)).Succeeded);
        Assert.True((await signIn.ExternalLoginSignInAsync("Google", "google-user-id", false)).Succeeded);
    }

    [Fact]
    public async Task MigrationApprovesExistingOwnerAndGrantsAdminAccessOnlyToOwner()
    {
        await using var provider = CreateServices();
        await using var scope = provider.CreateAsyncScope();
        var services = scope.ServiceProvider;
        var db = services.GetRequiredService<ApplicationDbContext>();
        await db.Database.OpenConnectionAsync();
        await db.GetService<IMigrator>().MigrateAsync("20260922135950_RequireUniqueEmail");
        await db.Database.ExecuteSqlRawAsync("""
            INSERT INTO AspNetUsers (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount)
            VALUES ('owner', 'justinmarkark@gmail.com', 'JUSTINMARKARK@GMAIL.COM', 'justinmarkark@gmail.com', 'JUSTINMARKARK@GMAIL.COM', 0, 0, 0, 0, 0),
                   ('other', 'other@example.com', 'OTHER@EXAMPLE.COM', 'other@example.com', 'OTHER@EXAMPLE.COM', 1, 0, 0, 0, 0);
            """);
        await db.Database.MigrateAsync();
        var manager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var owner = (await manager.FindByIdAsync("owner"))!;
        var other = (await manager.FindByIdAsync("other"))!;
        Assert.Equal(AccountApprovalStatus.Approved, owner.ApprovalStatus);
        Assert.Equal(AccountApprovalStatus.Pending, other.ApprovalStatus);
        Assert.True(await manager.IsInRoleAsync(owner, "SystemAdministrator"));
        Assert.False(await manager.IsInRoleAsync(other, "SystemAdministrator"));
        Assert.True((await manager.UpdateSecurityStampAsync(other)).Succeeded);

        var factory = services.GetRequiredService<IUserClaimsPrincipalFactory<ApplicationUser>>();
        var authorization = services.GetRequiredService<IAuthorizationService>();
        var attributes = typeof(AccountRequests).GetCustomAttributes(typeof(AuthorizeAttribute), true).Cast<AuthorizeAttribute>();
        var policy = (await AuthorizationPolicy.CombineAsync(services.GetRequiredService<IAuthorizationPolicyProvider>(), attributes))!;
        Assert.True((await authorization.AuthorizeAsync(await factory.CreateAsync(owner), null, policy)).Succeeded);
        Assert.False((await authorization.AuthorizeAsync(await factory.CreateAsync(other), null, policy)).Succeeded);
    }

    [Fact]
    public async Task RegisteringOwnerEmailAfterMigrationDoesNotGrantAdmin()
    {
        await using var provider = CreateServices();
        await using var scope = provider.CreateAsyncScope();
        await InitializeAsync(scope.ServiceProvider);
        var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = new ApplicationUser { UserName = "justinmarkark@gmail.com", Email = "justinmarkark@gmail.com" };
        Assert.True((await manager.CreateAsync(user)).Succeeded);
        Assert.Equal(AccountApprovalStatus.Pending, user.ApprovalStatus);
        Assert.False(await manager.IsInRoleAsync(user, "SystemAdministrator"));
    }

    private static ServiceProvider CreateServices()
    {
        var services = new ServiceCollection().AddLogging().AddAuthorization()
            .AddDbContext<ApplicationDbContext>(options => options.UseSqlite("Data Source=:memory:"));
        services.AddAuthentication(IdentityConstants.ApplicationScheme).AddIdentityCookies();
        services.AddDataProtection().UseEphemeralDataProtectionProvider();
        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedAccount = true;
                options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddUserConfirmation<AdministratorApprovalConfirmation>()
            .AddSignInManager();
        return services.BuildServiceProvider();
    }

    private static async Task InitializeAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        await db.Database.OpenConnectionAsync();
        await db.Database.MigrateAsync();
        services.GetRequiredService<SignInManager<ApplicationUser>>().Context = new DefaultHttpContext { RequestServices = services };
    }
}
