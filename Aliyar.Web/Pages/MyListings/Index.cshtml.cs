using Aliyar.Web.Data;
using Aliyar.Web.Models;
using Aliyar.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Aliyar.Web.Pages.MyListings;

[Authorize(Policy = AppPolicies.CustomerListing)]
public class IndexModel : PageModel
{
    private readonly AppDbContext _db;

    public IndexModel(AppDbContext db)
    {
        _db = db;
    }

    public List<ListingItem> Listings { get; private set; } = [];

    public List<ListingItem> ArchivedListings { get; private set; } = [];

    public async Task OnGetAsync()
    {
        var userId = User.GetUserId();
        if (userId is null)
            return;

        var cars = await _db.Cars.AsNoTracking()
            .Include(x => x.Specification)
            .Where(x => x.Kind == ListingKind.Customer && x.OwnerUserId == userId)
            .OrderByDescending(x => x.Id)
            .ToListAsync();

        Listings = cars
            .Where(x => !x.IsArchived)
            .Select(x => new ListingItem
            {
                Car = x,
                HasCompleteSpecifications = x.Specification?.HasRequiredFields() == true,
            })
            .ToList();

        ArchivedListings = cars
            .Where(x => x.IsArchived)
            .Select(x => new ListingItem
            {
                Car = x,
                HasCompleteSpecifications = x.Specification?.HasRequiredFields() == true,
            })
            .ToList();
    }

    public async Task<IActionResult> OnPostMarkSoldAsync(int id)
    {
        var userId = User.GetUserId();
        if (userId is null)
            return Unauthorized();

        var car = await _db.Cars.FirstOrDefaultAsync(x => x.Id == id);
        if (car is null || !car.IsOwnedBy(User))
            return NotFound();

        if (car.IsSold || car.IsArchived)
            return RedirectToPage();

        car.IsSold = true;
        await _db.SaveChangesAsync();
        TempData["StatusMessage"] = "Объявление отмечено как проданное.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostArchiveAsync(int id)
    {
        var car = await _db.Cars.FirstOrDefaultAsync(x => x.Id == id);
        if (car is null || !User.CanArchiveCar(car))
            return NotFound();

        CarArchive.Archive(car, User.GetUserId());
        await _db.SaveChangesAsync(HttpContext.RequestAborted);

        TempData["StatusMessage"] = "Объявление заархивировано.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRestoreAsync(int id)
    {
        var car = await _db.Cars.FirstOrDefaultAsync(x => x.Id == id);
        if (car is null || !User.CanRestoreCar(car))
            return NotFound();

        CarArchive.Restore(car);
        await _db.SaveChangesAsync(HttpContext.RequestAborted);

        TempData["StatusMessage"] = "Объявление восстановлено из архива.";
        return RedirectToPage();
    }

    public sealed class ListingItem
    {
        public required Car Car { get; init; }

        public bool HasCompleteSpecifications { get; init; }
    }
}
