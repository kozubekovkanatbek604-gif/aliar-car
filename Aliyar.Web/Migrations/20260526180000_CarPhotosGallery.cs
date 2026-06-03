using Aliyar.Web.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Aliyar.Web.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260526180000_CarPhotosGallery")]
    public partial class CarPhotosGallery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "car_photos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CarId = table.Column<int>(type: "integer", nullable: false),
                    Path = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_car_photos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_car_photos_car_shop_CarId",
                        column: x => x.CarId,
                        principalTable: "car_shop",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_car_photos_CarId",
                table: "car_photos",
                column: "CarId");

            migrationBuilder.CreateIndex(
                name: "IX_car_photos_CarId_SortOrder",
                table: "car_photos",
                columns: new[] { "CarId", "SortOrder" });

            migrationBuilder.Sql(
                """
                INSERT INTO car_photos ("CarId", "Path", "SortOrder", "CreatedAtUtc")
                SELECT "Id", "PhotoPath", 0, NOW() AT TIME ZONE 'UTC'
                FROM car_shop
                WHERE "PhotoPath" IS NOT NULL;
                """);

            migrationBuilder.DropColumn(
                name: "PhotoPath",
                table: "car_shop");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhotoPath",
                table: "car_shop",
                type: "character varying(260)",
                maxLength: 260,
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE car_shop c
                SET "PhotoPath" = p."Path"
                FROM (
                    SELECT DISTINCT ON ("CarId") "CarId", "Path"
                    FROM car_photos
                    ORDER BY "CarId", "SortOrder", "Id"
                ) p
                WHERE c."Id" = p."CarId";
                """);

            migrationBuilder.DropTable(
                name: "car_photos");
        }
    }
}
