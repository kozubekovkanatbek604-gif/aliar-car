using Aliyar.Web.Data;
using Aliyar.Web.Models;
using Aliyar.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Aliyar.Web.Pages.Listings;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;

    public IndexModel(AppDbContext db)
    {
        _db = db;
    }

    public List<Car> Listings { get; private set; } = [];

    public Dictionary<int, string> CoverPhotoUrls { get; private set; } = new();

    public string? ErrorMessage { get; private set; }

    [BindProperty(SupportsGet = true)]
    public CarCatalogFilter Filter { get; set; } = new();

    public async Task OnGetAsync()
    {
        try
        {
            var query = _db.Cars.AsNoTracking()
                .Include(x => x.Specification)
                .ActiveCustomerListings()
                .WithCompleteSpecifications();

            query = Filter.Apply(query);

            Listings = await query
                .OrderByDescending(x => x.Id)
                .ToListAsync();

            CoverPhotoUrls = await CarCatalogPhotos.GetCoverPhotoUrlsAsync(_db, Listings.Select(c => c.Id));
        }
        catch (Exception ex)
        {
            ErrorMessage = "Не удалось загрузить объявления.";
            Listings = [];
            Console.Error.WriteLine(ex);
        }
    }
}
