using Aliyar.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Aliyar.Web.Data;

public static class ClientDisplayName
{
    public static async Task<string?> GetFullNameAsync(AppDbContext db, string? userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(userId))
            return null;

        return await db.Clients
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => x.FullName)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
