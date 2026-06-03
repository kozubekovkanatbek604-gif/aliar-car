namespace Aliyar.Web.Services;

public sealed class CarPhotoStorage
{
    public const int MaxPhotosPerCar = 30;
    public const int MaxFilesPerUpload = 10;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp",
    };

    private const long MaxBytes = 5 * 1024 * 1024;

    private readonly IWebHostEnvironment _env;

    public CarPhotoStorage(IWebHostEnvironment env)
    {
        _env = env;
    }

    public static string ToPublicUrl(string photoPath) =>
        "/" + photoPath.TrimStart('/');

    public string? Validate(IFormFile file)
    {
        if (file.Length == 0)
            return "Пустой файл.";

        if (file.Length > MaxBytes)
            return $"«{file.FileName}»: размер не больше 5 МБ.";

        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
            return $"«{file.FileName}»: допустимы JPG, PNG, WEBP.";

        return null;
    }

    public async Task<string> SaveAsync(int carId, IFormFile file, CancellationToken ct = default)
    {
        var extension = Path.GetExtension(file.FileName)!.ToLowerInvariant();
        var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "cars");
        Directory.CreateDirectory(uploadsDir);

        var fileName = $"{carId}_{Guid.NewGuid():N}{extension}";
        var physicalPath = Path.Combine(uploadsDir, fileName);
        var relativePath = $"uploads/cars/{fileName}";

        await using var stream = File.Create(physicalPath);
        await file.CopyToAsync(stream, ct);

        return relativePath;
    }

    public void DeleteIfExists(string? photoPath)
    {
        if (string.IsNullOrWhiteSpace(photoPath))
            return;

        var physicalPath = Path.Combine(_env.WebRootPath, photoPath.Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(physicalPath))
            File.Delete(physicalPath);
    }
}
