namespace Aliyar.Web.Models;

public static class CarArchive
{
    public static void Archive(Car car, string? userId)
    {
        if (car.IsArchived)
            return;

        car.IsArchived = true;
        car.ArchivedAtUtc = DateTime.UtcNow;
        car.ArchivedByUserId = userId;
    }

    public static void Restore(Car car)
    {
        if (!car.IsArchived)
            return;

        car.IsArchived = false;
        car.ArchivedAtUtc = null;
        car.ArchivedByUserId = null;
    }
}
