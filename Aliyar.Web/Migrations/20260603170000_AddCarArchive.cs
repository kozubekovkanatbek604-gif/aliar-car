using Aliyar.Web.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aliyar.Web.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260603170000_AddCarArchive")]
    public partial class AddCarArchive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ArchivedAtUtc",
                table: "car_shop",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ArchivedByUserId",
                table: "car_shop",
                type: "character varying(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "car_shop",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_car_shop_IsArchived",
                table: "car_shop",
                column: "IsArchived");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_car_shop_IsArchived",
                table: "car_shop");

            migrationBuilder.DropColumn(
                name: "ArchivedAtUtc",
                table: "car_shop");

            migrationBuilder.DropColumn(
                name: "ArchivedByUserId",
                table: "car_shop");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "car_shop");
        }
    }
}
