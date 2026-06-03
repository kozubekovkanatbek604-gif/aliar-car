using System;
using Aliyar.Web.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Aliyar.Web.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260526120000_AddCarSales")]
    public partial class AddCarSales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSold",
                table: "car_shop",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "car_sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CarId = table.Column<int>(type: "integer", nullable: false),
                    CustomerName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SalePrice = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    SoldAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SoldByUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_car_sales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_car_sales_car_shop_CarId",
                        column: x => x.CarId,
                        principalTable: "car_shop",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_car_sales_CarId",
                table: "car_sales",
                column: "CarId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_car_sales_SoldAtUtc",
                table: "car_sales",
                column: "SoldAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "car_sales");

            migrationBuilder.DropColumn(
                name: "IsSold",
                table: "car_shop");
        }
    }
}
