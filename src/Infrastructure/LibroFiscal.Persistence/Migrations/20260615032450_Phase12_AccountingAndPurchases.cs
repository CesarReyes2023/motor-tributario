using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibroFiscal.Persistence.Migrations;

/// <inheritdoc />
public partial class Phase12AccountingAndPurchases : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Accounts",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Type = table.Column<int>(type: "integer", nullable: false),
                ParentAccountId = table.Column<Guid>(type: "uuid", nullable: true),
                IsTransactional = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Accounts", x => x.Id);
                table.ForeignKey(
                    name: "FK_Accounts_Accounts_ParentAccountId",
                    column: x => x.ParentAccountId,
                    principalTable: "Accounts",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "JournalEntries",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                Date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                Status = table.Column<int>(type: "integer", nullable: false),
                ReferenceDocumentId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_JournalEntries", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Purchases",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                SupplierNit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                SupplierNrc = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                SupplierName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                IssueDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                DocumentNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                SubTotal = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                TaxAmount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                TotalAmount = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                RecordedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                JournalEntryId = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Purchases", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "JournalEntryLines",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                JournalEntryId = table.Column<Guid>(type: "uuid", nullable: false),
                AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                Debit = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                Credit = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_JournalEntryLines", x => x.Id);
                table.ForeignKey(
                    name: "FK_JournalEntryLines_JournalEntries_JournalEntryId",
                    column: x => x.JournalEntryId,
                    principalTable: "JournalEntries",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

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

        migrationBuilder.CreateIndex(
            name: "IX_Accounts_CompanyId_Code",
            table: "Accounts",
            columns: new[] { "CompanyId", "Code" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Accounts_ParentAccountId",
            table: "Accounts",
            column: "ParentAccountId");

        migrationBuilder.CreateIndex(
            name: "IX_JournalEntryLines_JournalEntryId",
            table: "JournalEntryLines",
            column: "JournalEntryId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Accounts");

        migrationBuilder.DropTable(
            name: "JournalEntryLines");

        migrationBuilder.DropTable(
            name: "Purchases");

        migrationBuilder.DropTable(
            name: "JournalEntries");

        migrationBuilder.UpdateData(
            table: "TaxRules",
            keyColumn: "Id",
            keyValue: new Guid("a1a1a1a1-a1a1-a1a1-a1a1-a1a1a1a1a1a1"),
            column: "CreatedAt",
            value: new DateTimeOffset(new DateTime(2026, 6, 14, 15, 24, 48, 103, DateTimeKind.Unspecified).AddTicks(4987), new TimeSpan(0, 0, 0, 0, 0)));

        migrationBuilder.UpdateData(
            table: "TaxRules",
            keyColumn: "Id",
            keyValue: new Guid("b2b2b2b2-b2b2-b2b2-b2b2-b2b2b2b2b2b2"),
            column: "CreatedAt",
            value: new DateTimeOffset(new DateTime(2026, 6, 14, 15, 24, 48, 103, DateTimeKind.Unspecified).AddTicks(4999), new TimeSpan(0, 0, 0, 0, 0)));
    }
}
