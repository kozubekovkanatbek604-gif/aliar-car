using Aliyar.Web.Models;
using Aliyar.Web.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Aliyar.Web.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

        await EnsureRoleAsync(roleManager, AppRoles.Admin);
        await EnsureRoleAsync(roleManager, AppRoles.Manager);
        await EnsureRoleAsync(roleManager, AppRoles.Customer);
        await EnsureRoleAsync(roleManager, AppRoles.Client);

        // Default admin for admin pages (Cars CRUD). Credentials: admin@local / 123456
        const string adminEmail = "admin@local";
        const string adminPassword = "123456";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser is null)
        {
            adminUser = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
            var created = await userManager.CreateAsync(adminUser, adminPassword);
            if (created.Succeeded)
                await userManager.AddToRoleAsync(adminUser, AppRoles.Admin);
        }
        else if (!await userManager.IsInRoleAsync(adminUser, AppRoles.Admin))
        {
            await userManager.AddToRoleAsync(adminUser, AppRoles.Admin);
        }

        if (await db.Cars.AsNoTracking().AnyAsync())
            return;

        var cars = new[]
        {
            new Car { Kind = ListingKind.Store, Make = "Toyota", Model = "Camry", Year = 2018, Price = 18500, Vin = "TOY-CAM-2018-001" },
            new Car { Make = "Toyota", Model = "Corolla", Year = 2017, Price = 13900, Vin = "TOY-COR-2017-002" },
            new Car { Make = "Honda", Model = "Civic", Year = 2019, Price = 17900, Vin = "HON-CIV-2019-003" },
            new Car { Make = "Honda", Model = "Accord", Year = 2018, Price = 19200, Vin = "HON-ACC-2018-004" },
            new Car { Make = "Hyundai", Model = "Elantra", Year = 2020, Price = 16800, Vin = "HYU-ELA-2020-005" },
            new Car { Make = "Hyundai", Model = "Sonata", Year = 2019, Price = 18100, Vin = "HYU-SON-2019-006" },
            new Car { Make = "Kia", Model = "K5", Year = 2021, Price = 21900, Vin = "KIA-K5-2021-007" },
            new Car { Make = "Kia", Model = "Sportage", Year = 2018, Price = 17700, Vin = "KIA-SPO-2018-008" },
            new Car { Make = "Volkswagen", Model = "Passat", Year = 2017, Price = 14900, Vin = "VOL-PAS-2017-009" },
            new Car { Make = "Volkswagen", Model = "Golf", Year = 2016, Price = 12900, Vin = "VOL-GOL-2016-010" },
            new Car { Make = "BMW", Model = "320i", Year = 2016, Price = 19900, Vin = "BMW-320-2016-011" },
            new Car { Make = "Mercedes-Benz", Model = "C200", Year = 2016, Price = 20900, Vin = "MER-C20-2016-012" },
            new Car { Make = "Audi", Model = "A4", Year = 2017, Price = 21500, Vin = "AUD-A4-2017-013" },
            new Car { Make = "Ford", Model = "Focus", Year = 2018, Price = 13200, Vin = "FOR-FOC-2018-014" },
            new Car { Make = "Chevrolet", Model = "Cruze", Year = 2017, Price = 12100, Vin = "CHE-CRU-2017-015" },
        };

        db.Cars.AddRange(cars);
        await db.SaveChangesAsync();

        foreach (var car in cars)
            db.CarSpecifications.Add(CarSpecificationDefaults.ForStoreCar(car));

        await db.SaveChangesAsync();
    }

    private static async Task EnsureRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
    {
        if (await roleManager.RoleExistsAsync(roleName))
            return;

        await roleManager.CreateAsync(new IdentityRole(roleName));
    }
}

