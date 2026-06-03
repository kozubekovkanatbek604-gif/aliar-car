using Aliyar.Web.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aliyar.Web.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260603130000_AddCarPurchasePrice")]
    public partial class AddCarPurchasePrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PurchasePrice",
                table: "car_shop",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(
                """
                UPDATE car_shop
                SET "PurchasePrice" = GREATEST("Price" - 2000 - ("Id" % 1001), 0)
                WHERE "Kind" = 0;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PurchasePrice",
                table: "car_shop");
        }
    }
}
