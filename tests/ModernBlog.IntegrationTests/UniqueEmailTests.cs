using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModernBlog.Web.Data;

namespace ModernBlog.IntegrationTests;

public class UniqueEmailTests
{
    [Fact]
    public async Task IdentityRejectsDuplicateEmailForDifferentUserNames()
    {
        var services = new ServiceCollection()
            .AddLogging()
            .AddDbContext<ApplicationDbContext>(options => options.UseSqlite("Data Source=:memory:"));
        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>();
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.OpenConnectionAsync();
        await db.Database.MigrateAsync();
        var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        Assert.True((await manager.CreateAsync(new() { UserName = "first", Email = "person@example.com" })).Succeeded);
        var result = await manager.CreateAsync(new() { UserName = "second", Email = "PERSON@example.com" });

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, error => error.Code == "DuplicateEmail");

        var other = new ApplicationUser { UserName = "third", Email = "other@example.com" };
        Assert.True((await manager.CreateAsync(other)).Succeeded);
        result = await manager.SetEmailAsync(other, "person@example.com");
        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, error => error.Code == "DuplicateEmail");
    }

    [Fact]
    public async Task MigratedDatabaseRejectsDuplicateNormalizedEmail()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var services = new ServiceCollection().AddLogging()
            .AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connection));
        services.AddIdentityCore<ApplicationUser>(options => options.Stores.SchemaVersion = IdentitySchemaVersions.Version3)
            .AddEntityFrameworkStores<ApplicationDbContext>();
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();
        db.Users.Add(new() { UserName = "first", NormalizedUserName = "FIRST", Email = "person@example.com", NormalizedEmail = "PERSON@EXAMPLE.COM" });
        await db.SaveChangesAsync();
        db.Users.Add(new() { UserName = "second", NormalizedUserName = "SECOND", Email = "PERSON@example.com", NormalizedEmail = "PERSON@EXAMPLE.COM" });

        var exception = await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
        Assert.Equal(19, Assert.IsType<SqliteException>(exception.InnerException).SqliteErrorCode);
    }
}
