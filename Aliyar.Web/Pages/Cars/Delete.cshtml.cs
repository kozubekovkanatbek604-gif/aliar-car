using Aliyar.Web.Data;
using Aliyar.Web.Models;
using Aliyar.Web.Security;
using Aliyar.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Aliyar.Web.Pages.Cars;

[Authorize(Policy = AppPolicies.AdminOnly)]
public class DeleteModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly CarPhotoStorage _photos;

    public DeleteModel(AppDbContext db, CarPhotoStorage photos)
    {
        _db = db;
        _photos = photos;
    }

    public Car Car { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var car = await _db.Cars.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (car is null)
            return NotFound();

        Car = car;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var car = await _db.Cars.FirstOrDefaultAsync(x => x.Id == id);
        if (car is null)
            return RedirectToPage("Index");

        var photoPaths = await _db.CarPhotos
            .Where(x => x.CarId == id)
            .Select(x => x.Path)
            .ToListAsync();

        _db.Cars.Remove(car);
        try
        {
            await _db.SaveChangesAsync();
            foreach (var path in photoPaths)
                _photos.DeleteIfExists(path);
        }
        catch (DbUpdateException ex)
        {
            Console.Error.WriteLine(ex);
            return RedirectToPage("Details", new { id });
        }
        return RedirectToPage("Index");
    }
}
