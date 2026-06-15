using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibroFiscal.Persistence.Migrations;

/// <inheritdoc />
public partial class AddIdentityModule : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Users",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                PasswordHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                RoleId = table.Column<int>(type: "integer", nullable: false),
                IsActive = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Users", x => x.Id);
            });

        migrationBuilder.InsertData(
            table: "Users",
            columns: new[] { "Id", "IsActive", "PasswordHash", "RoleId", "Username" },
            values: new object[] { new Guid("11111111-1111-1111-1111-111111111111"), true, "NtVzQj7YD3Kouyuh4SCM+g==:0+0tEzq+CqbcJfG8K774w0zA3UAK6liYeC3eAnZezfo=", 1, "admin" });

        migrationBuilder.CreateIndex(
            name: "IX_Users_Username",
            table: "Users",
            column: "Username",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Users");
    }
}
