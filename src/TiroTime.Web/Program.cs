using System.IO.Compression;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using TiroTime.Application.Interfaces;
using TiroTime.Application.Services;
using TiroTime.Domain.Identity;
using TiroTime.Infrastructure;
using TiroTime.Infrastructure.Persistence;
using TiroTime.Web.Api;
using TiroTime.Web.Middleware;
using TiroTime.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Add Infrastructure services
builder.Services.AddInfrastructure(builder.Configuration);

// Add Application services
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddSingleton<ITimeEntryValidationService, TimeEntryValidationService>();

// Add Identity
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
    {
        // Password settings
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 6;
        options.Password.RequiredUniqueChars = 2;

        // Lockout settings
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromDays(365);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;

        // User settings
        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedEmail = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Configure cookie authentication
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

// Add Razor Pages
builder.Services.AddRazorPages();

// Response compression for dynamic HTML/JSON (static assets are pre-compressed at build time)
builder.Services.AddResponseCompression(options =>
{
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});
builder.Services.Configure<BrotliCompressionProviderOptions>(options => options.Level = CompressionLevel.Fastest);
builder.Services.Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.Fastest);

// Health checks (used by the container HEALTHCHECK and orchestrators)
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>("database");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseResponseCompression();

app.UseRouting();

app.UseAuthentication();

// Auto-login (only when exactly one user exists, see AutoLoginMiddleware)
app.UseMiddleware<AutoLoginMiddleware>();

app.UseAuthorization();

// Fingerprinted, pre-compressed static assets (replaces UseStaticFiles)
app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();
app.MapTimeEntriesApi();
app.MapReportsApi();
app.MapHealthChecks("/health").AllowAnonymous();

// Apply database migrations and seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();
        logger.LogInformation("Datenbank-Migrationen wurden erfolgreich angewendet");

        await DatabaseSeeder.SeedAsync(services);
        logger.LogInformation("Datenbank wurde erfolgreich initialisiert");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Fehler beim Initialisieren der Datenbank");
        throw;
    }
}

await app.RunAsync();
