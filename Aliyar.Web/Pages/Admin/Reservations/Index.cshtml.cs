using Aliyar.Web.Data;
using Aliyar.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Aliyar.Web.Pages.Admin.Reservations;

[Authorize(Policy = AppPolicies.CarManagement)]
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
        var now = DateTime.UtcNow;

        Items = await _db.CarReservations.AsNoTracking()
            .Where(x => x.Status == "Active" && x.ReservedUntilUtc > now)
            .OrderBy(x => x.ReservedUntilUtc)
            .Select(x => new Item
            {
                ReservationId = x.Id,
                CarId = x.CarId,
                CarTitle = x.Car.Make + " " + x.Car.Model,
                CustomerName = x.CustomerName,
                CustomerPhone = x.CustomerPhone,
                Quantity = x.Quantity,
                ReservedUntilLocal = x.ReservedUntilUtc.ToLocalTime().ToString("g"),
            })
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostCancelAsync(int reservationId)
    {
        var reservation = await _db.CarReservations.FirstOrDefaultAsync(x => x.Id == reservationId);
        if (reservation is null)
            return RedirectToPage();

        if (reservation.Status != "Active")
            return RedirectToPage();

        reservation.Status = "Cancelled";
        await _db.SaveChangesAsync(HttpContext.RequestAborted);

        TempData["StatusMessage"] = "Бронь отменена.";
        return RedirectToPage();
    }

    public sealed class Item
    {
        public int ReservationId { get; init; }
        public int CarId { get; init; }
        public string CarTitle { get; init; } = "";
        public string CustomerName { get; init; } = "";
        public string CustomerPhone { get; init; } = "";
        public int Quantity { get; init; }
        public string ReservedUntilLocal { get; init; } = "";
    }
}

