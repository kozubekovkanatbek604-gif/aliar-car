using Aliyar.Web.Data;
using Aliyar.Web.Models;
using Aliyar.Web.Security;
using Aliyar.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Aliyar.Web.Pages.Cars;

[Authorize(Policy = AppPolicies.CarManagement)]
public class UploadPhotoModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly CarPhotoStorage _photos;

    public UploadPhotoModel(AppDbContext db, CarPhotoStorage photos)
    {
        _db = db;
        _photos = photos;
    }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    public Car Car { get; private set; } = null!;

    public IReadOnlyList<CarPhoto> ExistingPhotos { get; private set; } = [];

    [BindProperty]
    public List<IFormFile>? NewPhotos { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var car = await LoadStoreCarAsync();
        if (car is null)
            return NotFound();

        Car = car;
        await LoadExistingPhotosAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var car = await _db.Cars.FirstOrDefaultAsync(x => x.Id == Id);
        if (car is null)
            return NotFound();

        if (car.IsCustomerListing())
            return NotFound();

        Car = car;
        await LoadExistingPhotosAsync();

        var files = NewPhotos?.Where(f => f.Length > 0).ToList() ?? [];
        if (files.Count == 0)
        {
            ModelState.AddModelError(nameof(NewPhotos), "Выберите один или несколько файлов.");
            return Page();
        }

        if (files.Count > CarPhotoStorage.MaxFilesPerUpload)
        {
            ModelState.AddModelError(nameof(NewPhotos), $"За раз можно загрузить не больше {CarPhotoStorage.MaxFilesPerUpload} файлов.");
            return Page();
        }

        var currentCount = await _db.CarPhotos.CountAsync(x => x.CarId == car.Id);
        if (currentCount + files.Count > CarPhotoStorage.MaxPhotosPerCar)
        {
            ModelState.AddModelError(nameof(NewPhotos),
                $"У автомобиля может быть не больше {CarPhotoStorage.MaxPhotosPerCar} фото (сейчас {currentCount}).");
            return Page();
        }

        var maxSort = await _db.CarPhotos
            .Where(x => x.CarId == car.Id)
            .Select(x => (int?)x.SortOrder)
            .MaxAsync() ?? -1;

        var added = 0;
        foreach (var file in files)
        {
            var validationError = _photos.Validate(file);
            if (validationError is not null)
            {
                ModelState.AddModelError(nameof(NewPhotos), validationError);
                continue;
            }

            string path;
            try
            {
                path = await _photos.SaveAsync(car.Id, file);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                ModelState.AddModelError(nameof(NewPhotos), $"Не удалось сохранить «{file.FileName}» на сервер.");
                continue;
            }

            maxSort++;
            _db.CarPhotos.Add(new CarPhoto
            {
                CarId = car.Id,
                Path = path,
                SortOrder = maxSort,
                CreatedAtUtc = DateTime.UtcNow,
            });
            added++;
        }

        if (added == 0)
            return Page();

        await _db.SaveChangesAsync();
        TempData["StatusMessage"] = added == 1 ? "Фото добавлено." : $"Добавлено фото: {added}.";
        return RedirectToPage("Details", new { id = car.Id });
    }

    public async Task<IActionResult> OnPostDeleteAsync(int photoId)
    {
        var photo = await _db.CarPhotos
            .Include(x => x.Car)
            .FirstOrDefaultAsync(x => x.Id == photoId && x.CarId == Id);

        if (photo is null || photo.Car.IsCustomerListing())
            return NotFound();

        var path = photo.Path;
        _db.CarPhotos.Remove(photo);
        await _db.SaveChangesAsync();
        _photos.DeleteIfExists(path);

        TempData["StatusMessage"] = "Фото удалено.";
        return RedirectToPage(new { id = Id });
    }

    private async Task LoadExistingPhotosAsync() =>
        ExistingPhotos = await _db.CarPhotos.AsNoTracking()
            .Where(x => x.CarId == Id)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Id)
            .ToListAsync();

    private async Task<Car?> LoadStoreCarAsync()
    {
        var car = await _db.Cars.AsNoTracking().FirstOrDefaultAsync(x => x.Id == Id);
        if (car is null || car.IsCustomerListing())
            return null;

        return car;
    }
}
