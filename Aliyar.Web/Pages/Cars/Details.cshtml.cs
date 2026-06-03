using Aliyar.Web.Data;
using Aliyar.Web.Models;
using Aliyar.Web.Security;
using Aliyar.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Aliyar.Web.Pages.Cars;

public class DetailsModel : PageModel
{
    private readonly AppDbContext _db;

    public DetailsModel(AppDbContext db)
    {
        _db = db;
    }

    public Car Car { get; private set; } = null!;

    public CarSale? Sale { get; private set; }

    public CarSpecification? Specification { get; private set; }

    public IReadOnlyList<string> PhotoUrls { get; private set; } = [];

    public CarReservation? ActiveReservation { get; private set; }

    [BindProperty]
    public ReserveInput Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var car = await _db.Cars.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (car is null)
            return NotFound();

        Car = car;

        Specification = await _db.CarSpecifications.AsNoTracking()
            .FirstOrDefaultAsync(x => x.CarId == id);

        PhotoUrls = await _db.CarPhotos.AsNoTracking()
            .Where(x => x.CarId == id)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Id)
            .Select(x => CarPhotoStorage.ToPublicUrl(x.Path))
            .ToListAsync();

        ActiveReservation = await _db.CarReservations.AsNoTracking()
            .Where(x => x.CarId == id && x.Status == "Active" && x.ReservedUntilUtc > DateTime.UtcNow)
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync();

        if (car.IsSold)
        {
            Sale = await _db.CarSales.AsNoTracking()
                .Where(x => x.CarId == id)
                .OrderByDescending(x => x.SoldAtUtc)
                .FirstOrDefaultAsync();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostArchiveAsync(int id)
    {
        if (!User.IsAppAdmin())
            return Forbid();

        var car = await _db.Cars.FirstOrDefaultAsync(x => x.Id == id);
        if (car is null || !User.CanArchiveCar(car))
            return NotFound();

        CarArchive.Archive(car, User.GetUserId());
        await _db.SaveChangesAsync(HttpContext.RequestAborted);

        TempData["StatusMessage"] = "Автомобиль заархивирован.";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostRestoreAsync(int id)
    {
        if (!User.IsAppAdmin())
            return Forbid();

        var car = await _db.Cars.FirstOrDefaultAsync(x => x.Id == id);
        if (car is null || !User.CanRestoreCar(car))
            return NotFound();

        CarArchive.Restore(car);
        await _db.SaveChangesAsync(HttpContext.RequestAborted);

        TempData["StatusMessage"] = "Автомобиль восстановлен из архива.";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostReserveAsync(int id)
    {
        var car = await _db.Cars.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (car is null)
            return NotFound();

        if (car.IsArchived)
        {
            TempData["StatusMessage"] = "Нельзя забронировать заархивированный автомобиль.";
            return RedirectToPage(new { id });
        }

        if (car.IsSold)
        {
            TempData["StatusMessage"] = "Нельзя забронировать проданный автомобиль.";
            return RedirectToPage(new { id });
        }

        if (car.IsStoreListing() && car.StockQuantity <= 0)
        {
            TempData["StatusMessage"] = "В наличии нет.";
            return RedirectToPage(new { id });
        }

        if (!ModelState.IsValid)
            return await OnGetAsync(id);

        var now = DateTime.UtcNow;
        var activeExists = await _db.CarReservations.AnyAsync(x =>
            x.CarId == id &&
            x.Status == "Active" &&
            x.ReservedUntilUtc > now);

        if (activeExists)
        {
            TempData["StatusMessage"] = "Этот автомобиль уже забронирован на время.";
            return RedirectToPage(new { id });
        }

        var hours = Input.DurationHours;
        if (hours is not (1 or 3 or 24 or 48))
            hours = 24;

        var qty = Math.Max(1, Input.Quantity);
        if (car.IsStoreListing())
            qty = Math.Min(qty, Math.Max(1, car.StockQuantity));
        else
            qty = 1;

        _db.CarReservations.Add(new CarReservation
        {
            CarId = id,
            CustomerName = Input.CustomerName.Trim(),
            CustomerPhone = Input.CustomerPhone.Trim(),
            CustomerDetails = string.IsNullOrWhiteSpace(Input.CustomerDetails) ? null : Input.CustomerDetails.Trim(),
            Quantity = qty,
            ReservedUntilUtc = now.AddHours(hours),
            CreatedAtUtc = now,
            Status = "Active",
        });

        await _db.SaveChangesAsync(HttpContext.RequestAborted);

        TempData["StatusMessage"] = "Автомобиль забронирован.";
        return RedirectToPage(new { id });
    }

    public sealed class ReserveInput
    {
        [Required(ErrorMessage = "Укажите имя клиента.")]
        [StringLength(200, ErrorMessage = "Имя не должно превышать {1} символов.")]
        [Display(Name = "Имя клиента")]
        public string CustomerName { get; set; } = "";

        [Required(ErrorMessage = "Укажите телефон клиента.")]
        [StringLength(50, ErrorMessage = "Телефон не должен превышать {1} символов.")]
        [Display(Name = "Телефон")]
        public string CustomerPhone { get; set; } = "";

        [StringLength(400, ErrorMessage = "Данные не должны превышать {1} символов.")]
        [Display(Name = "Данные клиента")]
        public string? CustomerDetails { get; set; }

        [Range(1, 999, ErrorMessage = "Количество должно быть не меньше {1}.")]
        [Display(Name = "Количество")]
        public int Quantity { get; set; } = 1;

        [Display(Name = "Срок брони")]
        public int DurationHours { get; set; } = 24;
    }
}
