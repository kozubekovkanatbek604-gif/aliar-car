namespace Aliyar.Web.Models;

public sealed class Car
{
    public int Id { get; set; }

    public string Make { get; set; } = "";

    public string Model { get; set; } = "";

    public int Year { get; set; }

    public int Price { get; set; }

    /// <summary>Цена закупки (только для автосалона).</summary>
    public int PurchasePrice { get; set; }

    public int StockQuantity { get; set; } = 1;

    public string? Vin { get; set; }

    public bool IsSold { get; set; }

    public bool IsArchived { get; set; }

    public DateTime? ArchivedAtUtc { get; set; }

    public string? ArchivedByUserId { get; set; }

    public ListingKind Kind { get; set; } = ListingKind.Store;

    public string? OwnerUserId { get; set; }

    public CarSpecification? Specification { get; set; }

    public ICollection<CarPhoto> Photos { get; set; } = new List<CarPhoto>();
}


