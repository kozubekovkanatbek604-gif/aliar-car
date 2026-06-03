using Aliyar.Web.Data;
using Aliyar.Web.Models;
using Aliyar.Web.Pages.Cars;
using Aliyar.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Aliyar.Web.Pages.MyListings;

[Authorize(Policy = AppPolicies.CustomerListing)]
public class EditModel : PageModel, ICarFormModel
{
    private readonly AppDbContext _db;

    public EditModel(AppDbContext db)
    {
        _db = db;
    }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    [BindProperty]
    public CarInputModel Car { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var entity = await LoadOwnedListingAsync();
        if (entity is null)
            return NotFound();

        if (entity.IsSold)
            return RedirectToPage("/Cars/Details", new { id = entity.Id });

        Car = new CarInputModel
        {
            Make = entity.Make,
            Model = entity.Model,
            Year = entity.Year,
            Price = entity.Price,
            Vin = entity.Vin,
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var entity = await _db.Cars.FirstOrDefaultAsync(x => x.Id == Id);
        if (entity is null || !entity.IsOwnedBy(User))
            return NotFound();

        if (entity.IsSold)
            return RedirectToPage("/Cars/Details", new { id = entity.Id });

        entity.Make = Car.Make.Trim();
        entity.Model = Car.Model.Trim();
        entity.Year = Car.Year;
        entity.Price = Car.Price;
        entity.Vin = string.IsNullOrWhiteSpace(Car.Vin) ? null : Car.Vin.Trim();

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            ModelState.AddModelError(string.Empty, "Не удалось сохранить. Проверьте уникальность VIN (если задан).");
            Console.Error.WriteLine(ex);
            return Page();
        }

        TempData["StatusMessage"] = "Объявление обновлено.";
        return RedirectToPage("Index");
    }

    private async Task<Car?> LoadOwnedListingAsync()
    {
        var entity = await _db.Cars.AsNoTracking().FirstOrDefaultAsync(x => x.Id == Id);
        if (entity is null || !entity.IsCustomerListing() || !entity.IsOwnedBy(User))
            return null;

        return entity;
    }
}
