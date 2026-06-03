using System.ComponentModel.DataAnnotations;
using Aliyar.Web.Data;
using Aliyar.Web.Models;
using Aliyar.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Aliyar.Web.Pages.Cars;

[Authorize(Policy = AppPolicies.CarManagement)]
public class EditNameModel : PageModel
{
    private readonly AppDbContext _db;

    public EditNameModel(AppDbContext db)
    {
        _db = db;
    }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    [BindProperty]
    public InputModel Car { get; set; } = new();

    public sealed class InputModel
    {
        [Required, StringLength(100)]
        public string Make { get; set; } = "";

        [Required, StringLength(100)]
        public string Model { get; set; } = "";
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var entity = await _db.Cars.AsNoTracking().FirstOrDefaultAsync(x => x.Id == Id);
        if (entity is null)
            return NotFound();

        if (entity.IsSold || entity.IsCustomerListing())
            return RedirectToPage("Details", new { id = entity.Id });

        Car = new InputModel { Make = entity.Make, Model = entity.Model };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var entity = await _db.Cars.FirstOrDefaultAsync(x => x.Id == Id);
        if (entity is null)
            return NotFound();

        if (entity.IsSold || entity.IsCustomerListing())
            return RedirectToPage("Details", new { id = entity.Id });

        entity.Make = Car.Make.Trim();
        entity.Model = Car.Model.Trim();

        await _db.SaveChangesAsync();
        return RedirectToPage("Details", new { id = entity.Id });
    }
}

