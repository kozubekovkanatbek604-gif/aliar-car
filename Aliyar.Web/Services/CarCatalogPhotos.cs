using Aliyar.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace Aliyar.Web.Services;

public static class CarCatalogPhotos
{
    public static async Task<Dictionary<int, string>> GetCoverPhotoUrlsAsync(
        AppDbContext db,
        IEnumerable<int> carIds,
        CancellationToken ct = default)
    {
        var ids = carIds.Distinct().ToList();
        if (ids.Count == 0)
            return new Dictionary<int, string>();

        var rows = await db.CarPhotos.AsNoTracking()
            .Where(p => ids.Contains(p.CarId))
            .OrderBy(p => p.SortOrder)
            .ThenBy(p => p.Id)
            .Select(p => new { p.CarId, p.Path })
            .ToListAsync(ct);

        return rows
            .GroupBy(x => x.CarId)
            .ToDictionary(
                g => g.Key,
                g => CarPhotoStorage.ToPublicUrl(g.First().Path));
    }
}
