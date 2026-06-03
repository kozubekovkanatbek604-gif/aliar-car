using Aliyar.Web.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aliyar.Web.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260528090100_AddStockQuantityAndSaleQuantity")]
    public partial class AddStockQuantityAndSaleQuantity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StockQuantity",
                table: "car_shop",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "car_sales",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.DropIndex(
                name: "IX_car_sales_CarId",
                table: "car_sales");

            migrationBuilder.CreateIndex(
                name: "IX_car_sales_CarId",
                table: "car_sales",
                column: "CarId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_car_sales_CarId",
                table: "car_sales");

            migrationBuilder.CreateIndex(
                name: "IX_car_sales_CarId",
                table: "car_sales",
                column: "CarId",
                unique: true);

            migrationBuilder.DropColumn(
                name: "StockQuantity",
                table: "car_shop");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "car_sales");
        }
    }
}

