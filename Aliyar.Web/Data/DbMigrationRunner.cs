using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Aliyar.Web.Data;

public static class DbMigrationRunner
{
    public static async Task ApplyPendingMigrationsAsync(string connectionString, CancellationToken ct = default)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
            .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning))
            .Options;

        await using var db = new AppDbContext(options);

        var pending = await db.Database.GetPendingMigrationsAsync(ct);
        var pendingList = pending.ToList();
        if (pendingList.Count > 0)
            Console.WriteLine($"Applying migrations: {string.Join(", ", pendingList)}");
        else
            Console.WriteLine("No pending migrations.");

        await db.Database.MigrateAsync(ct);
        Console.WriteLine("Database schema is up to date.");
    }
}
