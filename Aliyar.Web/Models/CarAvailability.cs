using Aliyar.Web.Security;

namespace Aliyar.Web.Models;

public static class CarAvailability
{
    public static bool IsOutOfStock(this Car car) =>
        car.IsStoreListing() && car.StockQuantity <= 0;

    public static bool IsCatalogAvailable(this Car car) =>
        !car.IsArchived && (car.IsCustomerListing() ? !car.IsSold : car.StockQuantity > 0);

    public static string CatalogStatusLabel(this Car car)
    {
        if (car.IsArchived)
            return "В архиве";

        return car.IsCustomerListing()
            ? (car.IsSold ? "Продан" : "В продаже")
            : (car.IsOutOfStock() ? "В наличии нет" : "В продаже");
    }
}
