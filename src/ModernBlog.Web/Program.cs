using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ModernBlog.Core.Markdown;
using ModernBlog.Core.Persistence;
using ModernBlog.Web.Components;
using ModernBlog.Web.Components.Account;
using ModernBlog.Web.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();
builder.Services.AddSingleton<ModernBlog.Core.Markdown.IMarkdownRenderer, MarkdownRenderer>();
builder.Services.AddHealthChecks();
builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
// Register the blog persistence context so pages/components can save posts
// Specify the migrations assembly so Database.Migrate() finds BlogDbContext migrations
builder.Services.AddDbContext<BlogDbContext>(options =>
    options.UseSqlite(connectionString, b => b.MigrationsAssembly("ModernBlog.Core")));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        // Allow sign in without confirmed account in development so users can log in
        // when no email sender is configured. Change to 'true' in production.
        options.SignIn.RequireConfirmedAccount = false;
        options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

var app = builder.Build();

// Ensure the SQLite data directory exists and the database is created so Identity can persist users.
// This uses EnsureCreated for development scenarios where migrations may not be present.
using (var scope = app.Services.CreateScope())
{
    var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
    var dataDir = Path.Combine(env.ContentRootPath, "Data");
    if (!Directory.Exists(dataDir)) Directory.CreateDirectory(dataDir);

    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
    // Apply any pending migrations for the blog context as well.
    var blogDb = scope.ServiceProvider.GetRequiredService<ModernBlog.Core.Persistence.BlogDbContext>();
    blogDb.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();
app.MapHealthChecks("/health");
    
app.Run();
