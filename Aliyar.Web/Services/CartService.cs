using System.Text.Json;

namespace Aliyar.Web.Services;

public sealed class CartService
{
    private const string SessionKey = "cart.v1";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public IReadOnlyList<CartItem> Get(HttpContext http)
    {
        var json = http.Session.GetString(SessionKey);
        if (string.IsNullOrWhiteSpace(json))
            return [];

        return JsonSerializer.Deserialize<List<CartItem>>(json, JsonOptions) ?? [];
    }

    public void Set(HttpContext http, IEnumerable<CartItem> items)
    {
        var list = items
            .Where(x => x.Quantity > 0)
            .GroupBy(x => x.CarId)
            .Select(g => new CartItem(g.Key, g.Sum(x => x.Quantity)))
            .ToList();

        http.Session.SetString(SessionKey, JsonSerializer.Serialize(list, JsonOptions));
    }

    public void Clear(HttpContext http) => http.Session.Remove(SessionKey);

    public sealed record CartItem(int CarId, int Quantity);
}

