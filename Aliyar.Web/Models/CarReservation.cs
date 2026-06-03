namespace Aliyar.Web.Models;

public sealed class CarReservation
{
    public int Id { get; set; }

    public int CarId { get; set; }

    public Car Car { get; set; } = null!;

    public string CustomerName { get; set; } = "";

    public string CustomerPhone { get; set; } = "";

    public string? CustomerDetails { get; set; }

    public int Quantity { get; set; } = 1;

    public DateTime ReservedUntilUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public string Status { get; set; } = "Active"; // Active, Cancelled, Expired
}

