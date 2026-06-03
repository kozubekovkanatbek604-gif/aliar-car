using Aliyar.Web.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aliyar.Web.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260526140000_AddCustomerListings")]
    public partial class AddCustomerListings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Kind",
                table: "car_shop",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "OwnerUserId",
                table: "car_shop",
                type: "character varying(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_car_shop_Kind",
                table: "car_shop",
                column: "Kind");

            migrationBuilder.CreateIndex(
                name: "IX_car_shop_OwnerUserId",
                table: "car_shop",
                column: "OwnerUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_car_shop_AspNetUsers_OwnerUserId",
                table: "car_shop",
                column: "OwnerUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_car_shop_AspNetUsers_OwnerUserId",
                table: "car_shop");

            migrationBuilder.DropIndex(
                name: "IX_car_shop_Kind",
                table: "car_shop");

            migrationBuilder.DropIndex(
                name: "IX_car_shop_OwnerUserId",
                table: "car_shop");

            migrationBuilder.DropColumn(
                name: "Kind",
                table: "car_shop");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "car_shop");
        }
    }
}
