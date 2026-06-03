namespace Aliyar.Web.Data;

public static class DatabaseConnection
{
    /// <summary>
    /// ConnectionStrings:Default или DATABASE_URL (Fly Managed Postgres).
    /// </summary>
    public static string Resolve(IConfiguration configuration)
    {
        var fromConfig = configuration.GetConnectionString("Default");
        if (!string.IsNullOrWhiteSpace(fromConfig))
            return fromConfig.Trim();

        var databaseUrl = configuration["DATABASE_URL"];
        if (!string.IsNullOrWhiteSpace(databaseUrl))
            return NormalizeDatabaseUrl(databaseUrl.Trim());

        throw new InvalidOperationException(
            "Не задана строка подключения к PostgreSQL. Укажите ConnectionStrings:Default " +
            "(секрет ConnectionStrings__Default на Fly) или переменную DATABASE_URL " +
            "(появляется после привязки Fly Managed Postgres).");
    }

    private static string NormalizeDatabaseUrl(string url)
    {
        // Npgsql понимает URI; Fly иногда отдаёт postgres:// вместо postgresql://
        if (url.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase))
            return "postgresql://" + url["postgres://".Length..];

        return url;
    }
}
