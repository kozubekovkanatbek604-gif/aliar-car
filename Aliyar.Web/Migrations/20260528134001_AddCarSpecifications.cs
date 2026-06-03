using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Aliyar.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddCarSpecifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "car_specifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CarId = table.Column<int>(type: "integer", nullable: false),
                    BodyType = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Mileage = table.Column<int>(type: "integer", nullable: false),
                    Color = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Doors = table.Column<int>(type: "integer", nullable: true),
                    Seats = table.Column<int>(type: "integer", nullable: true),
                    EngineVolumeLiters = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: true),
                    EnginePowerHp = table.Column<int>(type: "integer", nullable: true),
                    EngineType = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Transmission = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Drive = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    FuelConsumptionL100Km = table.Column<decimal>(type: "numeric(4,1)", precision: 4, scale: 1, nullable: true),
                    EmissionClass = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    HasAbs = table.Column<bool>(type: "boolean", nullable: false),
                    HasEsp = table.Column<bool>(type: "boolean", nullable: false),
                    HasAirbags = table.Column<bool>(type: "boolean", nullable: false),
                    HasAirConditioning = table.Column<bool>(type: "boolean", nullable: false),
                    HasClimateControl = table.Column<bool>(type: "boolean", nullable: false),
                    HasBluetooth = table.Column<bool>(type: "boolean", nullable: false),
                    HasUsb = table.Column<bool>(type: "boolean", nullable: false),
                    HasNavigation = table.Column<bool>(type: "boolean", nullable: false),
                    HasParkingSensors = table.Column<bool>(type: "boolean", nullable: false),
                    HasRearCamera = table.Column<bool>(type: "boolean", nullable: false),
                    Documents = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    OwnerCount = table.Column<int>(type: "integer", nullable: true),
                    Condition = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_car_specifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_car_specifications_car_shop_CarId",
                        column: x => x.CarId,
                        principalTable: "car_shop",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_car_specifications_BodyType",
                table: "car_specifications",
                column: "BodyType");

            migrationBuilder.CreateIndex(
                name: "IX_car_specifications_CarId",
                table: "car_specifications",
                column: "CarId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_car_specifications_Mileage",
                table: "car_specifications",
                column: "Mileage");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "car_specifications");
        }
    }
}
