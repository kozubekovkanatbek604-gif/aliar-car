using Aliyar.Web.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aliyar.Web.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260528160000_FixCarBodyTypes")]
    public partial class FixCarBodyTypes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE car_specifications s
                SET "BodyType" = inferred.type
                FROM car_shop c,
                LATERAL (
                    SELECT CASE
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
                        WHEN c."Model" ILIKE '%Sportage%'
                          OR c."Model" ILIKE '%Tiguan%'
                          OR c."Model" ILIKE '%RAV4%'
                          OR c."Model" ILIKE '%Tucson%'
                          OR c."Model" ILIKE '%Model Y%'
                          OR c."Model" ILIKE '%X1%'
                          OR c."Model" ILIKE '%X3%'
                          OR c."Model" ILIKE '%Q5%' THEN 3
                        WHEN c."Model" ILIKE '%Golf%'
                          OR c."Model" ILIKE '%Focus%'
                          OR c."Model" ILIKE '%Polo%'
                          OR c."Model" ILIKE '%Rio%'
                          OR c."Model" ILIKE '%Cooper%' THEN 2
                        WHEN c."Model" ILIKE '%Coupe%'
                          OR c."Model" ILIKE '%LC %'
                          OR c."Model" ILIKE '%A5%'
                          OR c."Model" ILIKE '%420%' THEN 4
                        WHEN c."Model" ILIKE '%Combi%'
                          OR c."Model" ILIKE '%Variant%'
                          OR c."Model" ILIKE '%V60%' THEN 5
                        WHEN c."Model" ILIKE '%Vito%'
                          OR c."Model" ILIKE '%Transit%'
                          OR c."Model" ILIKE '%Trafic%' THEN 6
                        WHEN c."Model" ILIKE '%Ranger%'
                          OR c."Model" ILIKE '%Hilux%'
                          OR c."Model" ILIKE '%L200%' THEN 7
                        ELSE 1
                    END AS type
                ) inferred
                WHERE s."CarId" = c."Id"
                  AND s."BodyType" IN (0, 8);
                """);

            migrationBuilder.Sql(
                """
                ALTER TABLE car_specifications
                ALTER COLUMN "BodyType" SET DEFAULT 1;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE car_specifications
                ALTER COLUMN "BodyType" SET DEFAULT 0;
                """);
        }
    }
}
