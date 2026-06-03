using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aliyar.Web.Migrations
{
    /// <inheritdoc />
    public partial class SyncCarShopModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_cars",
                table: "cars");

            migrationBuilder.RenameTable(
                name: "cars",
                newName: "car_shop");

            migrationBuilder.RenameIndex(
                name: "IX_cars_Vin",
                table: "car_shop",
                newName: "IX_car_shop_Vin");

            migrationBuilder.AddPrimaryKey(
                name: "PK_car_shop",
                table: "car_shop",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_car_shop",
                table: "car_shop");

            migrationBuilder.RenameTable(
                name: "car_shop",
                newName: "cars");

            migrationBuilder.RenameIndex(
                name: "IX_car_shop_Vin",
                table: "cars",
                newName: "IX_cars_Vin");

            migrationBuilder.AddPrimaryKey(
                name: "PK_cars",
                table: "cars",
                column: "Id");
        }
    }
}
