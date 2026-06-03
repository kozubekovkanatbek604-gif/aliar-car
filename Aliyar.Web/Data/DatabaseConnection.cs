using Npgsql;

namespace Aliyar.Web.Data;

public static class DatabaseConnection
{
    /// <summary>
    /// ConnectionStrings:Default или DATABASE_URL (Neon, Fly Postgres и т.д.).
    /// </summary>
    public static string Resolve(IConfiguration configuration)
    {
        var fromConfig = configuration.GetConnectionString("Default");
        if (!string.IsNullOrWhiteSpace(fromConfig))
            return ToNpgsqlConnectionString(fromConfig);

        var databaseUrl = configuration["DATABASE_URL"];
        if (!string.IsNullOrWhiteSpace(databaseUrl))
            return ToNpgsqlConnectionString(databaseUrl);

        throw new InvalidOperationException(
            "Не задана строка подключения к PostgreSQL. Укажите ConnectionStrings:Default " +
            "(секрет ConnectionStrings__Default на Fly) или переменную DATABASE_URL.");
    }

    private static string ToNpgsqlConnectionString(string value)
    {
        value = value.Trim().Trim('"', '\'');

        if (!IsPostgresUri(value))
            return value;

        if (value.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase))
            value = "postgresql://" + value["postgres://".Length..];

        var uri = new Uri(value);
        var userInfo = uri.UserInfo.Split(':', 2);
        var username = Uri.UnescapeDataString(userInfo[0]);
        var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port > 0 ? uri.Port : 5432,
            Database = uri.AbsolutePath.TrimStart('/'),
            Username = username,
            Password = password,
            SslMode = SslMode.Require,
        };

        if (!string.IsNullOrEmpty(uri.Query))
        {
            foreach (var part in uri.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                var kv = part.Split('=', 2);
                if (kv.Length != 2)
                    continue;

                var key = Uri.UnescapeDataString(kv[0]);
                var ssl = Uri.UnescapeDataString(kv[1]);
                if (key.Equals("sslmode", StringComparison.OrdinalIgnoreCase))
                {
                    builder.SslMode = ssl.ToLowerInvariant() switch
                    {
                        "require" => SslMode.Require,
                        "prefer" => SslMode.Prefer,
                        "disable" => SslMode.Disable,
                        "verify-full" => SslMode.VerifyFull,
                        "verify-ca" => SslMode.VerifyCA,
                        _ => SslMode.Require,
                    };
                }
            }
        }

        return builder.ConnectionString;
    }

    private static bool IsPostgresUri(string value) =>
        value.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase)
        || value.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase);
}
