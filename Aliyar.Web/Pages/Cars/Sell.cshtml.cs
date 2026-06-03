using Aliyar.Web.Data;
using Aliyar.Web.Models;
using Aliyar.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Aliyar.Web.Pages.Cars;

[Authorize(Policy = AppPolicies.CarManagement)]
public class SellModel : PageModel
{
    private readonly AppDbContext _db;

    public SellModel(AppDbContext db)
    {
        _db = db;
    }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    public Car Car { get; private set; } = null!;

    public SelectList ClientOptions { get; private set; } = null!;

    [BindProperty]
    public SaleInputModel Sale { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var car = await _db.Cars.AsNoTracking().FirstOrDefaultAsync(x => x.Id == Id);
        if (car is null)
            return NotFound();

        if (car.IsCustomerListing())
            return RedirectToPage("Details", new { id = car.Id });

        if (car.StockQuantity <= 0)
        {
            TempData["StatusMessage"] = "В наличии нет.";
            return RedirectToPage("Details", new { id = car.Id });
        }

        Car = car;
        Sale.SalePrice = car.Price;
        await LoadClientOptionsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var car = await _db.Cars.FirstOrDefaultAsync(x => x.Id == Id);
        if (car is null)
            return NotFound();

        if (car.IsCustomerListing())
            return RedirectToPage("Details", new { id = car.Id });

        if (car.StockQuantity <= 0)
        {
            TempData["StatusMessage"] = "В наличии нет.";
            return RedirectToPage("Details", new { id = car.Id });
        }

        Car = car;

        if (!ModelState.IsValid)
        {
            await LoadClientOptionsAsync();
            return Page();
        }

        var client = await _db.Clients.AsNoTracking().FirstOrDefaultAsync(x => x.Id == Sale.ClientId);
        if (client is null)
        {
            ModelState.AddModelError($"{nameof(Sale)}.{nameof(Sale.ClientId)}", "Выберите клиента из списка.");
            await LoadClientOptionsAsync();
            return Page();
        }

        car.StockQuantity--;
        if (car.StockQuantity < 0)
            car.StockQuantity = 0;

        _db.CarSales.Add(new CarSale
        {
            CarId = car.Id,
            ClientId = client.Id,
            CustomerName = client.FullName,
            Quantity = 1,
            SalePrice = Sale.SalePrice,
            PaymentMethod = Sale.PaymentMethod,
            SoldAtUtc = DateTime.UtcNow,
            SoldByUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
        });

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            ModelState.AddModelError(string.Empty, "Не удалось оформить продажу. Попробуйте ещё раз.");
            Console.Error.WriteLine(ex);
            await LoadClientOptionsAsync();
            return Page();
        }

        TempData["StatusMessage"] = $"Продажа оформлена: {car.Make} {car.Model}.";
        return RedirectToPage("Details", new { id = car.Id });
    }

    private async Task LoadClientOptionsAsync()
    {
        var items = await (
            from client in _db.Clients.AsNoTracking()
            join user in _db.Users.AsNoTracking() on client.UserId equals user.Id
            orderby client.FullName
            select new
            {
                client.Id,
                Label = $"{client.FullName} ({user.UserName})",
            }).ToListAsync();

        ClientOptions = new SelectList(items, "Id", "Label");
    }
}
