using Aliyar.Web.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aliyar.Web.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260528150000_SeedBodyTypeShowcaseCars")]
    public partial class SeedBodyTypeShowcaseCars : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                INSERT INTO car_shop ("Make", "Model", "Year", "Price", "StockQuantity", "Vin", "IsSold", "Kind")
                VALUES
                    ('Toyota', 'Camry', 2022, 24500, 2, 'ALI-BODY-SEDA-001', FALSE, 0),
                    ('Honda', 'Accord', 2021, 22800, 1, 'ALI-BODY-SEDA-002', FALSE, 0),
                    ('Hyundai', 'Sonata', 2020, 19500, 3, 'ALI-BODY-SEDA-003', FALSE, 0),
                    ('Volkswagen', 'Golf', 2021, 18900, 2, 'ALI-BODY-HATC-001', FALSE, 0),
                    ('Ford', 'Focus', 2019, 14200, 1, 'ALI-BODY-HATC-002', FALSE, 0),
                    ('Kia', 'Rio', 2022, 15600, 2, 'ALI-BODY-HATC-003', FALSE, 0),
                    ('Kia', 'Sportage', 2023, 26900, 2, 'ALI-BODY-SUV-001', FALSE, 0),
                    ('Toyota', 'RAV4', 2022, 28500, 1, 'ALI-BODY-SUV-002', FALSE, 0),
                    ('Hyundai', 'Tucson', 2021, 24800, 2, 'ALI-BODY-SUV-003', FALSE, 0),
                    ('BMW', '420i', 2020, 35900, 1, 'ALI-BODY-COUP-001', FALSE, 0),
                    ('Mercedes-Benz', 'C Coupe', 2019, 37200, 1, 'ALI-BODY-COUP-002', FALSE, 0),
                    ('Audi', 'A5', 2021, 38900, 1, 'ALI-BODY-COUP-003', FALSE, 0),
                    ('Volvo', 'V60', 2020, 32900, 1, 'ALI-BODY-WAGN-001', FALSE, 0),
                    ('Skoda', 'Octavia Combi', 2021, 19800, 2, 'ALI-BODY-WAGN-002', FALSE, 0),
                    ('Volkswagen', 'Passat Variant', 2019, 17500, 1, 'ALI-BODY-WAGN-003', FALSE, 0),
                    ('Mercedes-Benz', 'Vito', 2020, 28900, 1, 'ALI-BODY-VAN-001', FALSE, 0),
                    ('Ford', 'Transit', 2019, 22500, 2, 'ALI-BODY-VAN-002', FALSE, 0),
                    ('Renault', 'Trafic', 2021, 21200, 1, 'ALI-BODY-VAN-003', FALSE, 0),
                    ('Ford', 'Ranger', 2022, 31900, 2, 'ALI-BODY-PICK-001', FALSE, 0),
                    ('Toyota', 'Hilux', 2021, 33500, 1, 'ALI-BODY-PICK-002', FALSE, 0),
                    ('Mitsubishi', 'L200', 2020, 27800, 1, 'ALI-BODY-PICK-003', FALSE, 0),
                    ('Tesla', 'Model Y', 2023, 45900, 1, 'ALI-BODY-OTHR-001', FALSE, 0),
                    ('Lexus', 'LC 500', 2020, 89900, 1, 'ALI-BODY-OTHR-002', FALSE, 0),
                    ('Mini', 'Cooper', 2022, 22900, 2, 'ALI-BODY-OTHR-003', FALSE, 0);
                """);

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
                        WHEN c."Vin" LIKE 'ALI-BODY-SEDA-%' THEN 1
                        WHEN c."Vin" LIKE 'ALI-BODY-HATC-%' THEN 2
                        WHEN c."Vin" LIKE 'ALI-BODY-SUV-%' THEN 3
                        WHEN c."Vin" LIKE 'ALI-BODY-COUP-%' THEN 4
                        WHEN c."Vin" LIKE 'ALI-BODY-WAGN-%' THEN 5
                        WHEN c."Vin" LIKE 'ALI-BODY-VAN-%' THEN 6
                        WHEN c."Vin" LIKE 'ALI-BODY-PICK-%' THEN 7
                        WHEN c."Vin" = 'ALI-BODY-OTHR-001' THEN 3
                        WHEN c."Vin" = 'ALI-BODY-OTHR-002' THEN 4
                        WHEN c."Vin" = 'ALI-BODY-OTHR-003' THEN 2
                        ELSE 1
                    END,
                    GREATEST(0, (EXTRACT(YEAR FROM NOW())::int - c."Year") * 12000),
                    CASE (c."Id" % 6)
                        WHEN 0 THEN 'Белый'
                        WHEN 1 THEN 'Чёрный'
                        WHEN 2 THEN 'Серебристый'
                        WHEN 3 THEN 'Серый'
                        WHEN 4 THEN 'Синий'
                        ELSE 'Красный'
                    END,
                    CASE
                        WHEN c."Vin" LIKE 'ALI-BODY-VAN-%' THEN 5
                        WHEN c."Vin" LIKE 'ALI-BODY-COUP-%' THEN 2
                        WHEN c."Vin" LIKE 'ALI-BODY-PICK-%' THEN 4
                        ELSE 4
                    END,
                    CASE
                        WHEN c."Vin" LIKE 'ALI-BODY-VAN-%' THEN 8
                        WHEN c."Vin" LIKE 'ALI-BODY-PICK-%' THEN 5
                        ELSE 5
                    END,
                    CASE
                        WHEN c."Vin" = 'ALI-BODY-OTHR-001' THEN NULL
                        WHEN c."Vin" LIKE 'ALI-BODY-PICK-%' OR c."Vin" LIKE 'ALI-BODY-VAN-%' THEN 2.2
                        WHEN c."Make" IN ('BMW', 'Mercedes-Benz', 'Audi', 'Lexus') THEN 2.0
                        ELSE 1.6
                    END,
                    CASE
                        WHEN c."Vin" = 'ALI-BODY-OTHR-001' THEN 351
                        WHEN c."Vin" = 'ALI-BODY-OTHR-002' THEN 471
                        WHEN c."Make" IN ('BMW', 'Mercedes-Benz', 'Audi', 'Lexus') THEN 245
                        WHEN c."Vin" LIKE 'ALI-BODY-PICK-%' THEN 190
                        ELSE 130
                    END,
                    CASE
                        WHEN c."Vin" = 'ALI-BODY-OTHR-001' THEN 4
                        WHEN c."Vin" LIKE 'ALI-BODY-PICK-%' OR c."Vin" LIKE 'ALI-BODY-VAN-%' THEN 2
                        ELSE 1
                    END,
                    CASE
                        WHEN c."Vin" = 'ALI-BODY-OTHR-001' THEN 2
                        WHEN c."Vin" LIKE 'ALI-BODY-PICK-%' THEN 1
                        ELSE 2
                    END,
                    CASE
                        WHEN c."Vin" LIKE 'ALI-BODY-SUV-%' OR c."Vin" LIKE 'ALI-BODY-PICK-%' THEN 3
                        WHEN c."Make" IN ('BMW', 'Mercedes-Benz', 'Audi', 'Lexus') THEN 2
                        ELSE 1
                    END,
                    CASE
                        WHEN c."Vin" = 'ALI-BODY-OTHR-001' THEN NULL
                        WHEN c."Vin" LIKE 'ALI-BODY-PICK-%' THEN 9.5
                        WHEN c."Vin" LIKE 'ALI-BODY-VAN-%' THEN 8.2
                        ELSE 6.8
                    END,
                    CASE
                        WHEN c."Vin" = 'ALI-BODY-OTHR-001' THEN 'Euro-6'
                        ELSE 'Euro-5'
                    END,
                    TRUE, TRUE, TRUE, TRUE,
                    CASE WHEN c."Make" IN ('BMW', 'Mercedes-Benz', 'Audi', 'Lexus', 'Tesla') THEN TRUE ELSE FALSE END,
                    TRUE, TRUE,
                    CASE WHEN c."Make" IN ('BMW', 'Mercedes-Benz', 'Audi', 'Lexus', 'Tesla', 'Volvo') THEN TRUE ELSE FALSE END,
                    TRUE, TRUE,
                    'ПТС, VIN: ' || c."Vin",
                    1,
                    'Автосалон. ' || c."Make" || ' ' || c."Model" || ', ' || c."Year"::text || ' г.',
                    NOW() AT TIME ZONE 'UTC'
                FROM car_shop c
                WHERE c."Vin" LIKE 'ALI-BODY-%'
                  AND NOT EXISTS (
                      SELECT 1 FROM car_specifications s WHERE s."CarId" = c."Id"
                  );
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DELETE FROM car_shop
                WHERE "Vin" LIKE 'ALI-BODY-%';
                """);
        }
    }
}
