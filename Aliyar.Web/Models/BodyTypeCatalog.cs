namespace Aliyar.Web.Models;

public static class BodyTypeCatalog
{
    public const string AnyLabel = "Любой";

    public static IEnumerable<BodyType> FilterOptions()
    {
        yield return BodyType.Unknown;
        foreach (var bodyType in SelectableTypes())
            yield return bodyType;
    }

    public static IEnumerable<BodyType> SelectableTypes()
    {
        yield return BodyType.Sedan;
        yield return BodyType.Hatchback;
        yield return BodyType.Suv;
        yield return BodyType.Coupe;
        yield return BodyType.Wagon;
        yield return BodyType.Van;
        yield return BodyType.Pickup;
    }

    public static bool IsSelectable(BodyType bodyType) => bodyType is not BodyType.Unknown;

    public static string GetFilterLabel(BodyType bodyType) =>
        bodyType == BodyType.Unknown ? AnyLabel : bodyType.ToDisplayName();
}
