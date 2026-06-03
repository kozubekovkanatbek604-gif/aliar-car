using System.ComponentModel.DataAnnotations;
using Aliyar.Web.Data;
using Aliyar.Web.Models;
using Aliyar.Web.Security;
using Aliyar.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Aliyar.Web.Pages.Cart;

[Authorize(Policy = AppPolicies.CarManagement)]
public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly CartService _cart;

    public IndexModel(AppDbContext db, CartService cart)
    {
        _db = db;
        _cart = cart;
    }

    public List<CartLine> Items { get; private set; } = [];

    public int Total { get; private set; }

    public string? ErrorMessage { get; private set; }

    [TempData]
    public string? StatusMessage { get; set; }

    [BindProperty]
    public CheckoutInput Checkout { get; set; } = new();

    public async Task OnGetAsync()
    {
        await LoadAsync();
    }

    public async Task<IActionResult> OnPostAddAsync(int carId, int quantity = 1)
    {
        if (quantity <= 0)
            return RedirectToPage();

        var car = await _db.Cars.AsNoTracking().FirstOrDefaultAsync(x => x.Id == carId);
        if (car is null || !car.IsStoreListing())
            return NotFound();

        if (car.StockQuantity <= 0)
        {
            StatusMessage = "В наличии нет.";
            return RedirectToPage();
        }

        var cart = _cart.Get(HttpContext).ToList();
        var existing = cart.FirstOrDefault(x => x.CarId == carId);
        var nextQty = Math.Clamp((existing?.Quantity ?? 0) + quantity, 1, car.StockQuantity);

        cart.RemoveAll(x => x.CarId == carId);
        cart.Add(new CartService.CartItem(carId, nextQty));
        _cart.Set(HttpContext, cart);

        StatusMessage = "Добавлено в корзину.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUpdateAsync(int carId, int quantity)
    {
        var car = await _db.Cars.AsNoTracking().FirstOrDefaultAsync(x => x.Id == carId);
        if (car is null || !car.IsStoreListing())
            return NotFound();

        var cart = _cart.Get(HttpContext).ToList();
        cart.RemoveAll(x => x.CarId == carId);

        if (quantity > 0)
        {
            var clamped = Math.Clamp(quantity, 1, Math.Max(1, car.StockQuantity));
            cart.Add(new CartService.CartItem(carId, clamped));
        }

        _cart.Set(HttpContext, cart);
        return RedirectToPage();
    }

    public IActionResult OnPostRemove(int carId)
    {
        var cart = _cart.Get(HttpContext).ToList();
        cart.RemoveAll(x => x.CarId == carId);
        _cart.Set(HttpContext, cart);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCheckoutAsync()
    {
        var cart = _cart.Get(HttpContext).ToList();
        if (cart.Count == 0)
        {
            StatusMessage = "Корзина пуста.";
            return RedirectToPage();
        }

        if (!ModelState.IsValid)
        {
            await LoadAsync();
            return Page();
        }

        var carIds = cart.Select(x => x.CarId).Distinct().ToList();
        var cars = await _db.Cars.Where(x => carIds.Contains(x.Id)).ToListAsync();

        foreach (var item in cart)
        {
            var car = cars.FirstOrDefault(x => x.Id == item.CarId);
            if (car is null || !car.IsStoreListing())
            {
                ErrorMessage = "В корзине есть недоступный товар. Обновите корзину.";
                await LoadAsync();
                return Page();
            }

            if (car.StockQuantity < item.Quantity)
            {
                ErrorMessage = $"Недостаточно на складе: {car.Make} {car.Model}.";
                await LoadAsync();
                return Page();
            }
        }

        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            var sellerId = User.GetUserId();

            foreach (var item in cart)
            {
                var car = cars.First(x => x.Id == item.CarId);
                car.StockQuantity -= item.Quantity;
                if (car.StockQuantity < 0)
                    car.StockQuantity = 0;

                _db.CarSales.Add(new CarSale
                {
                    CarId = car.Id,
                    CustomerName = Checkout.CustomerName.Trim(),
                    Quantity = item.Quantity,
                    SalePrice = car.Price * item.Quantity,
                    PaymentMethod = Checkout.PaymentMethod,
                    SoldAtUtc = DateTime.UtcNow,
                    SoldByUserId = sellerId,
                });
            }

            await _db.SaveChangesAsync();
            await tx.CommitAsync();

            _cart.Clear(HttpContext);
            StatusMessage = "Продажа оформлена.";
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            ErrorMessage = "Не удалось оформить продажу. Попробуйте ещё раз.";
            Console.Error.WriteLine(ex);
            await LoadAsync();
            return Page();
        }
    }

    private async Task LoadAsync()
    {
        var cart = _cart.Get(HttpContext).ToList();
        if (cart.Count == 0)
        {
            Items = [];
            Total = 0;
            return;
        }

        var carIds = cart.Select(x => x.CarId).Distinct().ToList();
        var cars = await _db.Cars.AsNoTracking()
            .Where(x => carIds.Contains(x.Id))
            .ToListAsync();

        Items = cart
            .Join(cars, c => c.CarId, car => car.Id, (c, car) => new CartLine
            {
                CarId = car.Id,
                CarTitle = $"{car.Make} {car.Model} ({car.Year})",
                UnitPrice = car.Price,
                Quantity = Math.Min(c.Quantity, car.StockQuantity),
                StockQuantity = car.StockQuantity,
            })
            .OrderBy(x => x.CarTitle)
            .ToList();

        Total = Items.Sum(x => x.LineTotal);

        _cart.Set(HttpContext, Items.Select(x => new CartService.CartItem(x.CarId, x.Quantity)));
    }

    public sealed class CartLine
    {
        public int CarId { get; init; }
        public string CarTitle { get; init; } = "";
        public int UnitPrice { get; init; }
        public int Quantity { get; init; }
        public int StockQuantity { get; init; }
        public int LineTotal => UnitPrice * Quantity;
    }

    public sealed class CheckoutInput
    {
        [Required, StringLength(200, MinimumLength = 2)]
        [Display(Name = "Клиент")]
        public string CustomerName { get; set; } = "";

        [Display(Name = "Способ оплаты")]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    }
}

