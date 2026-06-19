using System;
using Microsoft.EntityFrameworkCore.Migrations;

#pragma warning disable CA1707
#nullable disable

namespace LibroFiscal.Persistence.Migrations;

/// <inheritdoc />
public partial class Phase21_AddApiPassword : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "ApiPassword",
            table: "Companies",
            type: "character varying(255)",
            maxLength: 255,
            nullable: false,
            defaultValue: "");

        migrationBuilder.UpdateData(
            table: "TaxRules",
            keyColumn: "Id",
            keyValue: new Guid("a1a1a1a1-a1a1-a1a1-a1a1-a1a1a1a1a1a1"),
            column: "CreatedAt",
            value: new DateTimeOffset(new DateTime(2026, 6, 16, 0, 40, 46, 43, DateTimeKind.Unspecified).AddTicks(9744), new TimeSpan(0, 0, 0, 0, 0)));

        migrationBuilder.UpdateData(
            table: "TaxRules",
            keyColumn: "Id",
            keyValue: new Guid("b2b2b2b2-b2b2-b2b2-b2b2-b2b2b2b2b2b2"),
            column: "CreatedAt",
            value: new DateTimeOffset(new DateTime(2026, 6, 16, 0, 40, 46, 43, DateTimeKind.Unspecified).AddTicks(9757), new TimeSpan(0, 0, 0, 0, 0)));
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "ApiPassword",
            table: "Companies");

        migrationBuilder.UpdateData(
            table: "TaxRules",
            keyColumn: "Id",
            keyValue: new Guid("a1a1a1a1-a1a1-a1a1-a1a1-a1a1a1a1a1a1"),
            column: "CreatedAt",
            value: new DateTimeOffset(new DateTime(2026, 6, 15, 3, 24, 49, 417, DateTimeKind.Unspecified).AddTicks(1142), new TimeSpan(0, 0, 0, 0, 0)));

        migrationBuilder.UpdateData(
            table: "TaxRules",
            keyColumn: "Id",
            keyValue: new Guid("b2b2b2b2-b2b2-b2b2-b2b2-b2b2b2b2b2b2"),
            column: "CreatedAt",
            value: new DateTimeOffset(new DateTime(2026, 6, 15, 3, 24, 49, 417, DateTimeKind.Unspecified).AddTicks(1157), new TimeSpan(0, 0, 0, 0, 0)));
    }
}
