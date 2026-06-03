using Aliyar.Web.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aliyar.Web.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260528140000_SeedStoreCarSpecifications")]
    public partial class SeedStoreCarSpecifications : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                INSERT INTO car_specifications (
                    "CarId", "BodyType", "Mileage", "Color", "Doors", "Seats",
                    "EngineVolumeLiters", "EnginePowerHp", "EngineType", "Transmission", "Drive",
                    "FuelConsumptionL100Km", "EmissionClass",
                    "HasAbs", "HasEsp", "HasAirbags", "HasAirConditioning", "HasClimateControl",
                    "HasBluetooth", "HasUsb", "HasNavigation", "HasParkingSensors", "HasRearCamera",
                    "Documents", "OwnerCount", "Condition", "UpdatedAtUtc"
                )
                SELECT
                    c."Id",
                    CASE
                        WHEN c."Model" ILIKE '%Sportage%'
                          OR c."Model" ILIKE '%Tiguan%'
                          OR c."Model" ILIKE '%RAV4%'
                          OR c."Model" ILIKE '%X1%'
                          OR c."Model" ILIKE '%X3%'
                          OR c."Model" ILIKE '%Q5%' THEN 3
                        WHEN c."Model" ILIKE '%Golf%'
                          OR c."Model" ILIKE '%Focus%'
                          OR c."Model" ILIKE '%Polo%' THEN 2
                        ELSE 1
                    END,
                    GREATEST(0, (EXTRACT(YEAR FROM NOW())::int - c."Year") * 12000),
                    CASE (c."Id" % 5)
                        WHEN 0 THEN 'Белый'
                        WHEN 1 THEN 'Чёрный'
                        WHEN 2 THEN 'Серебристый'
                        WHEN 3 THEN 'Серый'
                        ELSE 'Синий'
                    END,
                    4,
                    5,
                    CASE
                        WHEN c."Make" IN ('BMW', 'Mercedes-Benz', 'Audi') THEN 2.0
                        ELSE 1.6
                    END,
                    CASE
                        WHEN c."Make" IN ('BMW', 'Mercedes-Benz', 'Audi') THEN 184
                        ELSE 130
                    END,
                    1,
                    2,
                    CASE
                        WHEN c."Make" IN ('BMW', 'Mercedes-Benz', 'Audi') THEN 2
                        ELSE 1
                    END,
                    CASE
                        WHEN c."Make" IN ('BMW', 'Mercedes-Benz', 'Audi') THEN 7.5
                        ELSE 6.5
                    END,
                    'Euro-5',
                    TRUE, TRUE, TRUE, TRUE, FALSE,
                    TRUE, TRUE, FALSE, TRUE, FALSE,
                    CASE
                        WHEN c."Vin" IS NOT NULL AND c."Vin" <> '' THEN 'ПТС, VIN: ' || c."Vin"
                        ELSE 'ПТС'
                    END,
                    1,
                    'Автосалон. ' || c."Make" || ' ' || c."Model" || ', ' || c."Year"::text || ' г.',
                    NOW() AT TIME ZONE 'UTC'
                FROM car_shop c
                WHERE c."Kind" = 0
                  AND NOT EXISTS (
                      SELECT 1 FROM car_specifications s WHERE s."CarId" = c."Id"
                  );
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DELETE FROM car_specifications s
                USING car_shop c
                WHERE s."CarId" = c."Id"
                  AND c."Kind" = 0
                  AND s."Condition" LIKE 'Автосалон.%';
                """);
        }
    }
}
