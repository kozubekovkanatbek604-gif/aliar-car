using Aliyar.Web.Data;
using Aliyar.Web.Models;
using Aliyar.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Aliyar.Web.Pages.Cars;

[Authorize(Policy = AppPolicies.CarManagement)]
public class EditModel : PageModel, ICarFormModel
{
    private readonly AppDbContext _db;

    public EditModel(AppDbContext db)
    {
        _db = db;
    }

    public bool ShowPurchasePrice => true;

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    [BindProperty]
    public CarInputModel Car { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var entity = await _db.Cars.AsNoTracking().FirstOrDefaultAsync(x => x.Id == Id);
        if (entity is null)
            return NotFound();

        if (entity.IsSold || entity.IsArchived || entity.IsCustomerListing())
            return RedirectToPage("Details", new { id = entity.Id });

        Car = new CarInputModel
        {
            Make = entity.Make,
            Model = entity.Model,
            Year = entity.Year,
            Price = entity.Price,
            PurchasePrice = entity.PurchasePrice,
            Vin = entity.Vin,
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        if (Car.PurchasePrice <= 0)
        {
            ModelState.AddModelError("Car.PurchasePrice", "Укажите цену закупки.");
            return Page();
        }

        if (Car.PurchasePrice > Car.Price)
        {
            ModelState.AddModelError("Car.PurchasePrice", "Цена закупки не может быть выше цены продажи.");
            return Page();
        }

        var entity = await _db.Cars.FirstOrDefaultAsync(x => x.Id == Id);
        if (entity is null)
            return NotFound();

        if (entity.IsSold || entity.IsArchived || entity.IsCustomerListing())
            return RedirectToPage("Details", new { id = entity.Id });

        entity.Make = Car.Make.Trim();
        entity.Model = Car.Model.Trim();
        entity.Year = Car.Year;
        entity.Price = Car.Price;
        entity.PurchasePrice = Car.PurchasePrice;
        entity.Vin = string.IsNullOrWhiteSpace(Car.Vin) ? null : Car.Vin.Trim();

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            ModelState.AddModelError(string.Empty, "Не удалось сохранить. Проверь подключение к БД и уникальность VIN (если задан).");
            Console.Error.WriteLine(ex);
            return Page();
        }
        return RedirectToPage("Details", new { id = entity.Id });
    }
}

