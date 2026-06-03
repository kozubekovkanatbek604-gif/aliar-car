using Aliyar.Web.Data;
using Aliyar.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Aliyar.Web.Pages.Cars.Specifications;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;

    public IndexModel(AppDbContext db)
    {
        _db = db;
    }

    [BindProperty(SupportsGet = true)]
    public int CarId { get; set; }

    public Car Car { get; private set; } = null!;

    public CarSpecification? Specification { get; private set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var car = await _db.Cars.AsNoTracking()
            .Include(x => x.Specification)
            .FirstOrDefaultAsync(x => x.Id == CarId);

        if (car is null)
            return NotFound();

        Car = car;
        Specification = car.Specification;

        return Page();
    }
}
