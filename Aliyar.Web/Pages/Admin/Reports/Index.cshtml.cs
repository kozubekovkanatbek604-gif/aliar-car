using Aliyar.Web.Data;
using Aliyar.Web.Models;
using Aliyar.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Aliyar.Web.Pages.Admin.Reports;

[Authorize(Policy = AppPolicies.AdminOnly)]
public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;

    public IndexModel(AppDbContext db, UserManager<IdentityUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public int TotalSales { get; private set; }

    public int TotalRevenue { get; private set; }

    public int TotalProfit { get; private set; }

    public List<SalesReportRow> Rows { get; private set; } = [];

    public List<ManagerOption> Managers { get; private set; } = [];

    public string? ErrorMessage { get; private set; }

    [BindProperty(SupportsGet = true)]
    public string? ManagerUserId { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateOnly? DateFrom { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateOnly? DateTo { get; set; }

    public async Task OnGetAsync()
    {
        try
        {
            var query = _db.CarSales.AsNoTracking()
                .Include(x => x.Car)
                .AsQueryable();

            query = ApplyDateFilters(query, DateFrom, DateTo);

            var managerIds = await query
                .Select(x => x.SoldByUserId)
                .Where(x => !string.IsNullOrEmpty(x))
                .Distinct()
                .ToListAsync();

            var managerLabels = new Dictionary<string, string>();
            foreach (var userId in managerIds)
            {
                var user = await _userManager.FindByIdAsync(userId!);
                managerLabels[userId!] = user?.Email ?? user?.UserName ?? userId!;
            }

            Managers = managerIds
                .OrderBy(x => managerLabels.GetValueOrDefault(x!, x!))
                .Select(x => new ManagerOption(x!, managerLabels.GetValueOrDefault(x!, x!)))
                .ToList();

            query = ApplyManagerFilter(query, ManagerUserId);

            var sales = await query
                .OrderByDescending(x => x.SoldAtUtc)
                .ToListAsync();

            TotalSales = sales.Count;
            TotalRevenue = sales.Sum(x => x.SalePrice);
            TotalProfit = sales.Sum(x => CarProfit.NetProfit(x.Car, x));

            var sellerIds = sales
                .Select(x => x.SoldByUserId)
                .Where(x => !string.IsNullOrEmpty(x))
                .Distinct()
                .ToList();

            var sellerEmails = new Dictionary<string, string>();
            foreach (var userId in sellerIds)
            {
                var user = await _userManager.FindByIdAsync(userId!);
                if (user?.Email is not null)
                    sellerEmails[userId!] = user.Email;
            }

            Rows = sales.Select(x => new SalesReportRow
            {
                Id = x.Id,
                SoldAtUtc = x.SoldAtUtc,
                CarTitle = $"{x.Car.Make} {x.Car.Model} ({x.Car.Year})",
                CustomerName = x.CustomerName,
                SalePrice = x.SalePrice,
                NetProfit = CarProfit.NetProfit(x.Car, x),
                PaymentMethod = x.PaymentMethod,
                SoldBy = x.SoldByUserId is null
                    ? "—"
                    : sellerEmails.GetValueOrDefault(x.SoldByUserId, x.SoldByUserId),
            }).ToList();
        }
        catch (Exception ex)
        {
            ErrorMessage = "Не удалось загрузить отчёт. Проверьте подключение к БД и миграции.";
            Console.Error.WriteLine(ex);
        }
    }

    private static IQueryable<CarSale> ApplyManagerFilter(IQueryable<CarSale> query, string? managerUserId)
    {
        if (string.IsNullOrWhiteSpace(managerUserId))
            return query;

        if (managerUserId == "__none__")
            return query.Where(x => x.SoldByUserId == null);

        return query.Where(x => x.SoldByUserId == managerUserId);
    }

    private static IQueryable<CarSale> ApplyDateFilters(IQueryable<CarSale> query, DateOnly? dateFrom, DateOnly? dateTo)
    {
        if (dateFrom is not null)
        {
            var fromUtc = new DateTime(dateFrom.Value.Year, dateFrom.Value.Month, dateFrom.Value.Day, 0, 0, 0, DateTimeKind.Utc);
            query = query.Where(x => x.SoldAtUtc >= fromUtc);
        }

        if (dateTo is not null)
        {
            var toExclusiveUtc = new DateTime(dateTo.Value.Year, dateTo.Value.Month, dateTo.Value.Day, 0, 0, 0, DateTimeKind.Utc)
                .AddDays(1);
            query = query.Where(x => x.SoldAtUtc < toExclusiveUtc);
        }

        return query;
    }

    public sealed class SalesReportRow
    {
        public int Id { get; init; }

        public DateTime SoldAtUtc { get; init; }

        public string CarTitle { get; init; } = "";

        public string CustomerName { get; init; } = "";

        public int SalePrice { get; init; }

        public int NetProfit { get; init; }

        public PaymentMethod PaymentMethod { get; init; }

        public string SoldBy { get; init; } = "";
    }

    public sealed record ManagerOption(string UserId, string Label);
}
