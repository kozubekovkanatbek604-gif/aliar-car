using Aliyar.Web.Models;

namespace Aliyar.Web.Data;

public static class CarQueryExtensions
{
    public static IQueryable<Car> StoreInventory(this IQueryable<Car> query) =>
        query.Where(x => x.Kind == ListingKind.Store && !x.IsArchived);

    public static IQueryable<Car> ActiveCustomerListings(this IQueryable<Car> query) =>
        query.Where(x => x.Kind == ListingKind.Customer && !x.IsSold && !x.IsArchived);

    public static IQueryable<Car> ArchivedCars(this IQueryable<Car> query) =>
        query.Where(x => x.IsArchived);

    public static IQueryable<Car> WithCompleteSpecifications(this IQueryable<Car> query) =>
        query.Where(x => x.Specification != null
            && x.Specification.BodyType != BodyType.Unknown
            && x.Specification.EngineType != EngineType.Unknown
            && x.Specification.Transmission != TransmissionType.Unknown
            && x.Specification.Drive != CarDriveType.Unknown
            && x.Specification.EngineVolumeLiters != null
            && x.Specification.EnginePowerHp != null
            && x.Specification.Color != "");
}
