using Aliyar.Web.Data;
using Aliyar.Web.Models;
using Aliyar.Web.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Aliyar.Web.Pages.Cars.Specifications;

public class EditModel : PageModel
{
    private readonly AppDbContext _db;

    public EditModel(AppDbContext db)
    {
        _db = db;
    }

    [BindProperty(SupportsGet = true)]
    public int CarId { get; set; }

    public Car Car { get; private set; } = null!;

    public bool IsRequiredForClient { get; private set; }

    [BindProperty]
    public SpecificationInputModel Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var car = await LoadCarAsync();
        if (car is null)
            return NotFound();

        if (!User.CanEditCarSpecifications(car))
            return Forbid();

        Car = car;
        IsRequiredForClient = RequiresClientValidation(car);

        if (car.Specification is not null)
            Input = SpecificationInputModel.FromEntity(car.Specification);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var car = await _db.Cars
            .Include(x => x.Specification)
            .FirstOrDefaultAsync(x => x.Id == CarId);

        if (car is null)
            return NotFound();

        if (!User.CanEditCarSpecifications(car))
            return Forbid();

        Car = car;
        IsRequiredForClient = RequiresClientValidation(car);

        if (IsRequiredForClient)
            ValidateRequiredForClient();

        if (!ModelState.IsValid)
            return Page();

        var spec = car.Specification ?? new CarSpecification { CarId = car.Id };
        Input.ApplyTo(spec);

        if (car.Specification is null)
            _db.CarSpecifications.Add(spec);

        await _db.SaveChangesAsync(HttpContext.RequestAborted);

        TempData["StatusMessage"] = "Технические характеристики сохранены.";
        return RedirectToPage("Index", new { carId = car.Id });
    }

    private bool RequiresClientValidation(Car car) =>
        car.IsCustomerListing() && car.IsOwnedBy(User);

    private void ValidateRequiredForClient()
    {
        if (!BodyTypeCatalog.IsSelectable(Input.BodyType))
            ModelState.AddModelError($"{nameof(Input)}.{nameof(Input.BodyType)}", "Укажите тип кузова.");

        if (Input.Mileage < 0)
            ModelState.AddModelError($"{nameof(Input)}.{nameof(Input.Mileage)}", "Укажите пробег.");

        if (string.IsNullOrWhiteSpace(Input.Color))
            ModelState.AddModelError($"{nameof(Input)}.{nameof(Input.Color)}", "Укажите цвет.");

        if (Input.EngineVolumeLiters is null or <= 0)
            ModelState.AddModelError($"{nameof(Input)}.{nameof(Input.EngineVolumeLiters)}", "Укажите объём двигателя.");

        if (Input.EnginePowerHp is null or <= 0)
            ModelState.AddModelError($"{nameof(Input)}.{nameof(Input.EnginePowerHp)}", "Укажите мощность двигателя.");

        if (Input.EngineType == EngineType.Unknown)
            ModelState.AddModelError($"{nameof(Input)}.{nameof(Input.EngineType)}", "Укажите тип двигателя.");

        if (Input.Transmission == TransmissionType.Unknown)
            ModelState.AddModelError($"{nameof(Input)}.{nameof(Input.Transmission)}", "Укажите коробку передач.");

        if (Input.Drive == CarDriveType.Unknown)
            ModelState.AddModelError($"{nameof(Input)}.{nameof(Input.Drive)}", "Укажите привод.");
    }

    private async Task<Car?> LoadCarAsync() =>
        await _db.Cars.AsNoTracking()
            .Include(x => x.Specification)
            .FirstOrDefaultAsync(x => x.Id == CarId);
}
