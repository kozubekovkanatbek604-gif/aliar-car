using System.Globalization;

namespace Aliyar.Web;

public static class MoneyFormat
{
    public static string Usd(int amount) =>
        string.Create(CultureInfo.CurrentCulture, $"${amount:N0}");
}
