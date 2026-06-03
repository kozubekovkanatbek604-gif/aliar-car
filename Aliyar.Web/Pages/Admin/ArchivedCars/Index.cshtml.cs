using Aliyar.Web.Data;
using Aliyar.Web.Models;
using Aliyar.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Aliyar.Web.Pages.Admin.ArchivedCars;

[Authorize(Policy = AppPolicies.AdminOnly)]
public class IndexModel : PageModel
{
    private readonly AppDbContext _db;

    public IndexModel(AppDbContext db)
    {
        _db = db;
    }

    public List<Item> Items { get; private set; } = [];

    public async Task OnGetAsync()
    {
        Items = await _db.Cars.AsNoTracking()
            .ArchivedCars()
            .OrderByDescending(x => x.ArchivedAtUtc)
            .ThenByDescending(x => x.Id)
            .Select(x => new Item
            {
                CarId = x.Id,
                Make = x.Make,
                Model = x.Model,
                Year = x.Year,
                Price = x.Price,
                KindLabel = x.Kind == ListingKind.Store ? "Автосалон" : "Объявление",
                ArchivedAtLocal = x.ArchivedAtUtc.HasValue
                    ? x.ArchivedAtUtc.Value.ToLocalTime().ToString("g")
                    : "—",
            })
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostRestoreAsync(int id)
    {
        var car = await _db.Cars.FirstOrDefaultAsync(x => x.Id == id);
        if (car is null || !car.IsArchived)
            return RedirectToPage();

        CarArchive.Restore(car);
        await _db.SaveChangesAsync(HttpContext.RequestAborted);

        TempData["StatusMessage"] = "Запись восстановлена из архива.";
        return RedirectToPage();
    }

    public sealed class Item
    {
        public int CarId { get; init; }

        public string Make { get; init; } = "";

        public string Model { get; init; } = "";

        public int Year { get; init; }

        public int Price { get; init; }

        public string KindLabel { get; init; } = "";

        public string ArchivedAtLocal { get; init; } = "";
    }
}
