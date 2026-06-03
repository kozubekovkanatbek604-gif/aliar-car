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
public class CreateModel : PageModel, ICarFormModel
{
    private readonly AppDbContext _db;

    public CreateModel(AppDbContext db)
    {
        _db = db;
    }

    [BindProperty]
    public CarInputModel Car { get; set; } = new() { Year = DateTime.UtcNow.Year };

    public async Task<IActionResult> OnPostAsync()
    {
        var userId = User.GetUserId();
        if (userId is null)
            return Unauthorized();

        if (!ModelState.IsValid)
            return Page();

        var entity = new Car
        {
            Kind = ListingKind.Customer,
            OwnerUserId = userId,
            Make = Car.Make.Trim(),
            Model = Car.Model.Trim(),
            Year = Car.Year,
            Price = Car.Price,
            Vin = string.IsNullOrWhiteSpace(Car.Vin) ? null : Car.Vin.Trim(),
        };

        _db.Cars.Add(entity);
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

        TempData["StatusMessage"] = "Объявление опубликовано. Заполните технические характеристики.";
        return RedirectToPage("/Cars/Specifications/Edit", new { carId = entity.Id });
    }
}
