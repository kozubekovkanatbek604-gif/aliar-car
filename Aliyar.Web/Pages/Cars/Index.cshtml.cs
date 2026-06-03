using Aliyar.Web.Data;
using Aliyar.Web.Models;
using Aliyar.Web.Security;
using Aliyar.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Aliyar.Web.Pages.Cars;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;

    public IndexModel(AppDbContext db)
    {
        _db = db;
    }

    public List<Car> Cars { get; private set; } = [];

    public Dictionary<int, string> CoverPhotoUrls { get; private set; } = new();

    public string? ErrorMessage { get; private set; }

    [BindProperty(SupportsGet = true)]
    public CarCatalogFilter Filter { get; set; } = new();

    public async Task OnGet()
    {
        try
        {
            var query = _db.Cars.AsNoTracking()
                .Include(x => x.Specification)
                .StoreInventory();

            query = Filter.Apply(query);

            Cars = await query
                .OrderByDescending(x => x.Id)
                .ToListAsync();

            CoverPhotoUrls = await CarCatalogPhotos.GetCoverPhotoUrlsAsync(_db, Cars.Select(c => c.Id));
        }
        catch (Exception ex)
        {
            ErrorMessage = "Не удалось загрузить список. Проверь подключение к PostgreSQL и миграции.";
            Cars = [];
            Console.Error.WriteLine(ex);
        }
    }

    public async Task<IActionResult> OnPostArchiveAsync(int id)
    {
        if (!User.IsAppAdmin())
            return Forbid();

        var car = await _db.Cars.FirstOrDefaultAsync(x => x.Id == id);
        if (car is null || !User.CanArchiveCar(car))
            return RedirectToPage();

        CarArchive.Archive(car, User.GetUserId());
        await _db.SaveChangesAsync(HttpContext.RequestAborted);

        TempData["StatusMessage"] = "Автомобиль заархивирован.";
        return RedirectToPage();
    }
}
