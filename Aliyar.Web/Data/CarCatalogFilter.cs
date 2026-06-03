using Aliyar.Web.Models;

namespace Aliyar.Web.Data;

public sealed class CarCatalogFilter
{
    public const int DefaultMinYear = 1980;

    public const int DefaultMaxYear = 2025;

    public const int YearStep = 5;

    public const int DefaultMinPrice = 10_000;

    public const int DefaultMaxPrice = 100_000;

    public const int PriceStep = 1_000;

    public const int MinMileageOption = 5_000;

    public const int MaxMileageOption = 150_000;

    public const int DefaultMaxMileage = 100_000;

    public const int MileageStep = 5_000;

    public int? MinYear { get; set; }

    public int? MaxYear { get; set; }

    public int? MinPrice { get; set; }

    public int? MaxPrice { get; set; }

    public int? MaxMileage { get; set; }

    public BodyType? BodyTypeFilter { get; set; }

    public int EffectiveMinYear => MinYear ?? DefaultMinYear;

    public int EffectiveMaxYear => MaxYear ?? DefaultMaxYear;

    public int EffectiveMinPrice => MinPrice ?? DefaultMinPrice;

    public int EffectiveMaxPrice => MaxPrice ?? DefaultMaxPrice;

    public int EffectiveMaxMileage => MaxMileage ?? DefaultMaxMileage;

    public static IEnumerable<int> YearOptions()
    {
        for (var year = DefaultMinYear; year <= DefaultMaxYear; year += YearStep)
            yield return year;
    }

    public static IEnumerable<int> PriceOptions()
    {
        for (var price = DefaultMinPrice; price <= DefaultMaxPrice; price += PriceStep)
            yield return price;
    }

    public static IEnumerable<int> MileageOptions()
    {
        for (var mileage = MinMileageOption; mileage <= MaxMileageOption; mileage += MileageStep)
            yield return mileage;
    }

    public IQueryable<Car> Apply(IQueryable<Car> query)
    {
        query = query.Where(x => x.Year >= EffectiveMinYear);
        query = query.Where(x => x.Year <= EffectiveMaxYear);
        query = query.Where(x => x.Price >= EffectiveMinPrice);
        query = query.Where(x => x.Price <= EffectiveMaxPrice);

        query = query.Where(x => x.Specification != null && x.Specification.Mileage <= EffectiveMaxMileage);

        if (BodyTypeFilter.HasValue && BodyTypeFilter.Value != BodyType.Unknown)
            query = query.Where(x => x.Specification != null && x.Specification.BodyType == BodyTypeFilter.Value);

        return query;
    }
}
