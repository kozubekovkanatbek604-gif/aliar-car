using System.ComponentModel.DataAnnotations;
using Aliyar.Web.Data;
using Aliyar.Web.Models;
using Aliyar.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Aliyar.Web.Pages.Cars;

[Authorize(Policy = AppPolicies.AdminOnly)]
public class EditStockModel : PageModel
{
    private readonly AppDbContext _db;

    public EditStockModel(AppDbContext db)
    {
        _db = db;
    }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    public string CarTitle { get; private set; } = "";

    [BindProperty]
    public InputModel Car { get; set; } = new();

    public sealed class InputModel
    {
        [Range(0, 9999, ErrorMessage = "Количество должно быть от {1} до {2}.")]
        [Display(Name = "На складе")]
        public int StockQuantity { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var entity = await _db.Cars.AsNoTracking().FirstOrDefaultAsync(x => x.Id == Id);
        if (entity is null)
            return NotFound();

        if (!entity.IsStoreListing())
            return RedirectToPage("Details", new { id = entity.Id });

        CarTitle = $"{entity.Make} {entity.Model}";
        Car = new InputModel { StockQuantity = entity.StockQuantity };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var entity = await _db.Cars.AsNoTracking().FirstOrDefaultAsync(x => x.Id == Id);
        if (entity is null)
            return NotFound();

        if (!entity.IsStoreListing())
            return RedirectToPage("Details", new { id = entity.Id });

        CarTitle = $"{entity.Make} {entity.Model}";

        if (!ModelState.IsValid)
            return Page();

        var tracked = await _db.Cars.FirstOrDefaultAsync(x => x.Id == Id);
        if (tracked is null)
            return NotFound();

        tracked.StockQuantity = Car.StockQuantity;
        if (tracked.StockQuantity > 0 && tracked.IsSold)
            tracked.IsSold = false;

        await _db.SaveChangesAsync(HttpContext.RequestAborted);

        TempData["StatusMessage"] = "Количество на складе обновлено.";
        return RedirectToPage("Details", new { id = tracked.Id });
    }
}
