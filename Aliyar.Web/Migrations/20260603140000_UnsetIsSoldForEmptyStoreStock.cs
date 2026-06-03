using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aliyar.Web.Migrations;

/// <inheritdoc />
public partial class UnsetIsSoldForEmptyStoreStock : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            UPDATE car_shop
            SET "IsSold" = false
            WHERE "Kind" = 0 AND "StockQuantity" <= 0 AND "IsSold" = true;
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
    }
}
