using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibroFiscal.Persistence.Migrations;

/// <inheritdoc />
public partial class Phase21ExpandAddressLength : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "Departamento",
            table: "Companies",
            type: "character varying(100)",
            maxLength: 100,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "character varying(2)",
            oldMaxLength: 2);

        migrationBuilder.AlterColumn<string>(
            name: "Municipio",
            table: "Companies",
            type: "character varying(100)",
            maxLength: 100,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "character varying(2)",
            oldMaxLength: 2);

        migrationBuilder.AlterColumn<string>(
            name: "Departamento",
            table: "Establishments",
            type: "character varying(100)",
            maxLength: 100,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "character varying(2)",
            oldMaxLength: 2);

        migrationBuilder.AlterColumn<string>(
            name: "Municipio",
            table: "Establishments",
            type: "character varying(100)",
            maxLength: 100,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "character varying(2)",
            oldMaxLength: 2);

        migrationBuilder.UpdateData(
            table: "TaxRules",
            keyColumn: "Id",
            keyValue: new Guid("a1a1a1a1-a1a1-a1a1-a1a1-a1a1a1a1a1a1"),
            column: "CreatedAt",
            value: new DateTimeOffset(new DateTime(2026, 6, 16, 5, 3, 19, 135, DateTimeKind.Unspecified).AddTicks(7618), new TimeSpan(0, 0, 0, 0, 0)));

        migrationBuilder.UpdateData(
            table: "TaxRules",
            keyColumn: "Id",
            keyValue: new Guid("b2b2b2b2-b2b2-b2b2-b2b2-b2b2b2b2b2b2"),
            column: "CreatedAt",
            value: new DateTimeOffset(new DateTime(2026, 6, 16, 5, 3, 19, 135, DateTimeKind.Unspecified).AddTicks(7629), new TimeSpan(0, 0, 0, 0, 0)));
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.UpdateData(
            table: "TaxRules",
            keyColumn: "Id",
            keyValue: new Guid("a1a1a1a1-a1a1-a1a1-a1a1-a1a1a1a1a1a1"),
            column: "CreatedAt",
            value: new DateTimeOffset(new DateTime(2026, 6, 16, 4, 57, 53, 208, DateTimeKind.Unspecified).AddTicks(7846), new TimeSpan(0, 0, 0, 0, 0)));

        migrationBuilder.UpdateData(
            table: "TaxRules",
            keyColumn: "Id",
            keyValue: new Guid("b2b2b2b2-b2b2-b2b2-b2b2-b2b2b2b2b2b2"),
            column: "CreatedAt",
            value: new DateTimeOffset(new DateTime(2026, 6, 16, 4, 57, 53, 208, DateTimeKind.Unspecified).AddTicks(7869), new TimeSpan(0, 0, 0, 0, 0)));
    }
}

