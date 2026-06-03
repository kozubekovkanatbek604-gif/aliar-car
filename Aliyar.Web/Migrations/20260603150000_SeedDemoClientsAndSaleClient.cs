using Aliyar.Web.Data;
using Aliyar.Web.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aliyar.Web.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260603150000_SeedDemoClientsAndSaleClient")]
public partial class SeedDemoClientsAndSaleClient : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "ClientId",
            table: "car_sales",
            type: "integer",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_car_sales_ClientId",
            table: "car_sales",
            column: "ClientId");

        migrationBuilder.AddForeignKey(
            name: "FK_car_sales_clients_ClientId",
            table: "car_sales",
            column: "ClientId",
            principalTable: "clients",
            principalColumn: "Id",
            onDelete: ReferentialAction.SetNull);

        SeedDemoClients(migrationBuilder);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        foreach (var client in DemoClientsSeedData.Clients)
        {
            migrationBuilder.Sql(
                $"""
                DELETE FROM clients WHERE "UserId" = '{client.UserId}';
                DELETE FROM "AspNetUserRoles" WHERE "UserId" = '{client.UserId}';
                DELETE FROM "AspNetUsers" WHERE "Id" = '{client.UserId}';
                """);
        }

        migrationBuilder.DropForeignKey(
            name: "FK_car_sales_clients_ClientId",
            table: "car_sales");

        migrationBuilder.DropIndex(
            name: "IX_car_sales_ClientId",
            table: "car_sales");

        migrationBuilder.DropColumn(
            name: "ClientId",
            table: "car_sales");
    }

    private static void SeedDemoClients(MigrationBuilder migrationBuilder)
    {
        var hasher = new PasswordHasher<IdentityUser>();
        var passwordHash = hasher.HashPassword(new IdentityUser(), DemoClientsSeedData.Password)
            .Replace("'", "''", StringComparison.Ordinal);

        migrationBuilder.Sql(
            $"""
            INSERT INTO "AspNetRoles" ("Id", "Name", "NormalizedName", "ConcurrencyStamp")
            SELECT '{DemoClientsSeedData.ClientRoleId}', '{AppRoles.Client}', '{AppRoles.Client.ToUpperInvariant()}', '{Guid.NewGuid()}'
            WHERE NOT EXISTS (
                SELECT 1 FROM "AspNetRoles" WHERE "NormalizedName" = '{AppRoles.Client.ToUpperInvariant()}'
            );
            """);

        foreach (var client in DemoClientsSeedData.Clients)
        {
            var normalizedLogin = client.Login.ToUpperInvariant();
            var securityStamp = Guid.NewGuid().ToString();
            var concurrencyStamp = Guid.NewGuid().ToString();
            var escapedName = client.FullName.Replace("'", "''", StringComparison.Ordinal);

            migrationBuilder.Sql(
                $"""
                INSERT INTO "AspNetUsers" (
                    "Id", "UserName", "NormalizedUserName", "EmailConfirmed",
                    "PasswordHash", "SecurityStamp", "ConcurrencyStamp",
                    "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnabled", "AccessFailedCount"
                )
                SELECT
                    '{client.UserId}',
                    '{client.Login}',
                    '{normalizedLogin}',
                    TRUE,
                    '{passwordHash}',
                    '{securityStamp}',
                    '{concurrencyStamp}',
                    FALSE,
                    FALSE,
                    TRUE,
                    0
                WHERE NOT EXISTS (
                    SELECT 1 FROM "AspNetUsers" WHERE "NormalizedUserName" = '{normalizedLogin}'
                );

                INSERT INTO "AspNetUserRoles" ("UserId", "RoleId")
                SELECT '{client.UserId}', r."Id"
                FROM "AspNetRoles" r
                WHERE r."NormalizedName" = '{AppRoles.Client.ToUpperInvariant()}'
                  AND NOT EXISTS (
                      SELECT 1 FROM "AspNetUserRoles" ur
                      WHERE ur."UserId" = '{client.UserId}' AND ur."RoleId" = r."Id"
                  );

                INSERT INTO clients ("FullName", "UserId")
                SELECT '{escapedName}', '{client.UserId}'
                WHERE NOT EXISTS (
                    SELECT 1 FROM clients WHERE "UserId" = '{client.UserId}'
                );
                """);
        }
    }
}
