namespace Aliyar.Web.Models;

public sealed class CarSale
{
    public int Id { get; set; }

    public int CarId { get; set; }

    public Car Car { get; set; } = null!;

    public string CustomerName { get; set; } = "";

    public int? ClientId { get; set; }

    public Client? Client { get; set; }

    public int SalePrice { get; set; }

    public int Quantity { get; set; } = 1;

    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

    public DateTime SoldAtUtc { get; set; }

    public string? SoldByUserId { get; set; }
}
