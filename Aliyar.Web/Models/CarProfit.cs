namespace Aliyar.Web.Models;

public static class CarProfit
{
    public static int UnitMargin(Car car) => car.Price - car.PurchasePrice;

    public static int NetProfit(Car car, CarSale sale) =>
        sale.SalePrice - car.PurchasePrice * Math.Max(1, sale.Quantity);
}
