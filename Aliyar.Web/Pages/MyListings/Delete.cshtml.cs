using Aliyar.Web.Data;
using Aliyar.Web.Models;
using Aliyar.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Aliyar.Web.Pages.MyListings;

[Authorize(Policy = AppPolicies.CustomerListing)]
public class DeleteModel : PageModel
{
    private readonly AppDbContext _db;

    public DeleteModel(AppDbContext db)
    {
        _db = db;
    }

    public Car Car { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var car = await _db.Cars.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (car is null || !car.IsOwnedBy(User))
            return NotFound();

        Car = car;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var car = await _db.Cars.FirstOrDefaultAsync(x => x.Id == id);
        if (car is null || !car.IsOwnedBy(User))
            return RedirectToPage("Index");

        _db.Cars.Remove(car);
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            Console.Error.WriteLine(ex);
            return RedirectToPage("/Cars/Details", new { id });
        }

        TempData["StatusMessage"] = "Объявление удалено.";
        return RedirectToPage("Index");
    }
}
