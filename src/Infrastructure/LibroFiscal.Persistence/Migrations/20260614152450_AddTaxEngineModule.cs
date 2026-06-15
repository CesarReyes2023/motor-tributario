using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LibroFiscal.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTaxEngineModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TaxRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Rate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    TypeId = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    LastModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxRules", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "TaxRules",
                columns: new[] { "Id", "Code", "CreatedAt", "CreatedBy", "IsActive", "LastModifiedAt", "LastModifiedBy", "Name", "Rate", "TypeId" },
                values: new object[,]
                {
                    { new Guid("a1a1a1a1-a1a1-a1a1-a1a1-a1a1a1a1a1a1"), "IVA", new DateTimeOffset(new DateTime(2026, 6, 14, 15, 24, 48, 103, DateTimeKind.Unspecified).AddTicks(4987), new TimeSpan(0, 0, 0, 0, 0)), "System", true, null, null, "Impuesto al Valor Agregado", 13.00m, 1 },
                    { new Guid("b2b2b2b2-b2b2-b2b2-b2b2-b2b2b2b2b2b2"), "RET_1", new DateTimeOffset(new DateTime(2026, 6, 14, 15, 24, 48, 103, DateTimeKind.Unspecified).AddTicks(4999), new TimeSpan(0, 0, 0, 0, 0)), "System", false, null, null, "Retención Gran Contribuyente", 1.00m, 2 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaxRules");
        }
    }
}
