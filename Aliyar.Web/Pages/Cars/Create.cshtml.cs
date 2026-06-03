using Aliyar.Web.Data;
using Aliyar.Web.Models;
using Aliyar.Web.Pages.Cars.Specifications;
using Aliyar.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Aliyar.Web.Pages.Cars;

[Authorize(Policy = AppPolicies.CarManagement)]
public class CreateModel : PageModel, ICarFormModel
{
    private readonly AppDbContext _db;

    public CreateModel(AppDbContext db)
    {
        _db = db;
    }

    public bool ShowPurchasePrice => true;

    [BindProperty]
    public CarInputModel Car { get; set; } = new() { Year = DateTime.UtcNow.Year };

    [BindProperty]
    public SpecificationInputModel Specification { get; set; } = new();

    public async Task<IActionResult> OnPostAsync()
    {
        Specification.AddRequiredFieldErrors(ModelState, nameof(Specification));

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

        var entity = new Car
        {
            Kind = ListingKind.Store,
            Make = Car.Make.Trim(),
            Model = Car.Model.Trim(),
            Year = Car.Year,
            Price = Car.Price,
            PurchasePrice = Car.PurchasePrice,
            Vin = string.IsNullOrWhiteSpace(Car.Vin) ? null : Car.Vin.Trim(),
        };

        _db.Cars.Add(entity);
        try
        {
            await _db.SaveChangesAsync();

            var spec = new CarSpecification { CarId = entity.Id };
            Specification.ApplyTo(spec);
            _db.CarSpecifications.Add(spec);
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            ModelState.AddModelError(string.Empty, "Не удалось сохранить. Проверь подключение к БД и уникальность VIN (если задан).");
            Console.Error.WriteLine(ex);
            return Page();
        }

        TempData["StatusMessage"] = "Автомобиль добавлен в каталог.";
        return RedirectToPage("Details", new { id = entity.Id });
    }
}

