using Aliyar.Web.Data;
using Aliyar.Web.Security;
using Aliyar.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

var connectionString = DatabaseConnection.Resolve(builder.Configuration);

if (args.Contains("--db-migrate", StringComparer.OrdinalIgnoreCase))
{
    await DbMigrationRunner.ApplyPendingMigrationsAsync(connectionString);
    return;
}

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
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
