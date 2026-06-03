using Aliyar.Web.Data;
using Aliyar.Web.Security;
using Aliyar.Web.Services;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

if (args.Contains("--db-migrate", StringComparer.OrdinalIgnoreCase))
{
    try
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{env}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var cs = DatabaseConnection.Resolve(config);
        await DbMigrationRunner.ApplyPendingMigrationsAsync(cs);
        Console.WriteLine("Release migration completed successfully.");
        Environment.Exit(0);
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine("Release migration failed. Set Fly secret ConnectionStrings__Default to your PostgreSQL (Neon/Supabase/etc.):");
        Console.Error.WriteLine(ex);
        Environment.Exit(1);
    }
}

var builder = WebApplication.CreateBuilder(args);

var connectionString = DatabaseConnection.Resolve(builder.Configuration);

Console.WriteLine("Applying database migrations...");
await DbMigrationRunner.ApplyPendingMigrationsAsync(connectionString);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddSingleton<CarPhotoStorage>();
builder.Services.AddSingleton<ManagerPhotoStorage>();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".Aliyar.Cart";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromHours(8);
});
builder.Services.AddScoped<CartService>();

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 52_428_800; // 50 MB — запас для нескольких фото по 5 MB
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 52_428_800;
});

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(connectionString);
    options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
});

builder.Services
    .AddDefaultIdentity<IdentityUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.User.RequireUniqueEmail = false;
        options.Password.RequiredLength = 6;
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AppPolicies.CarManagement, policy =>
        policy.RequireRole(AppRoles.Admin, AppRoles.Manager));
    options.AddPolicy(AppPolicies.AdminOnly, policy =>
        policy.RequireRole(AppRoles.Admin));
    options.AddPolicy(AppPolicies.CustomerListing, policy =>
        policy.RequireRole(AppRoles.Customer, AppRoles.Client));
});

var app = builder.Build();

try
{
    await DbSeeder.SeedAsync(app.Services);
}
catch (Exception ex)
{
    Console.Error.WriteLine("Database migration or seed failed:");
    Console.Error.WriteLine(ex);
    throw;
}

// Configure the HTTP request pipeline.
app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    // Fly уже отдаёт HTTPS снаружи; редирект ломает внутренние health checks по HTTP :8080.
    if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("FLY_APP_NAME")))
        app.UseHttpsRedirection();
}

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// Загруженные в runtime файлы (uploads/*) не входят в MapStaticAssets — отдаём через wwwroot.
app.UseStaticFiles();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
