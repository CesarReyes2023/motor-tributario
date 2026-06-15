using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibroFiscal.Persistence.Migrations;

/// <inheritdoc />
public partial class InitialPostgres : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Companies",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                RazonSocial = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                NombreComercial = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Nit = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
                Nrc = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                CodigoActividad = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                DescripcionActividad = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                Telefono = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                Correo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Departamento = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                Municipio = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                DireccionComplemento = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                AmbienteId = table.Column<int>(type: "integer", nullable: false),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                LastModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                LastModifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Companies", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Dtes",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                TipoDteId = table.Column<int>(type: "integer", nullable: false),
                Version = table.Column<int>(type: "integer", nullable: false),
                CodigoGeneracion = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                NumeroControl = table.Column<string>(type: "character varying(31)", maxLength: 31, nullable: false),
                FechaEmision = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                FechaTransmision = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                FechaAnulacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                EstadoId = table.Column<int>(type: "integer", nullable: false),
                SelloRecepcion = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                FirmaElectronica = table.Column<string>(type: "text", nullable: true),
                JsonOriginal = table.Column<string>(type: "text", nullable: true),
                JsonFirmado = table.Column<string>(type: "text", nullable: true),
                MotivoRechazo = table.Column<string>(type: "text", nullable: true),
                IntentosTransmision = table.Column<int>(type: "integer", nullable: false),
                AmbienteId = table.Column<int>(type: "integer", nullable: false),
                ModeloFacturacionId = table.Column<int>(type: "integer", nullable: false),
                TipoTransmisionId = table.Column<int>(type: "integer", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                LastModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                LastModifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                CuerpoDocumento = table.Column<string>(type: "jsonb", nullable: true),
                Emisor = table.Column<string>(type: "jsonb", nullable: false),
                HistorialEstados = table.Column<string>(type: "jsonb", nullable: true),
                Receptor = table.Column<string>(type: "jsonb", nullable: true),
                Resumen = table.Column<string>(type: "jsonb", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Dtes", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "LibrosIva",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                TipoLibroId = table.Column<int>(type: "integer", nullable: false),
                FiscalYear = table.Column<int>(type: "integer", nullable: false),
                FiscalMonth = table.Column<int>(type: "integer", nullable: false),
                EstadoId = table.Column<int>(type: "integer", nullable: false),
                TotalGravado = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                TotalExento = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                TotalNoSujeto = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                TotalIva = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                TotalGeneral = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                LastModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                LastModifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                Entradas = table.Column<string>(type: "jsonb", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_LibrosIva", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Establishments",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                Codigo = table.Column<string>(type: "character varying(4)", maxLength: 4, nullable: false),
                Nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                PuntoVenta = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                Departamento = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                Municipio = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                DireccionComplemento = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Telefono = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                Correo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                IsActive = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Establishments", x => x.Id);
                table.ForeignKey(
                    name: "FK_Establishments_Companies_CompanyId",
                    column: x => x.CompanyId,
                    principalTable: "Companies",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Dtes_CodigoGeneracion",
            table: "Dtes",
            column: "CodigoGeneracion",
            unique: true,
            filter: "\"CodigoGeneracion\" IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_Dtes_CompanyId",
            table: "Dtes",
            column: "CompanyId");

        migrationBuilder.CreateIndex(
            name: "IX_Dtes_CompanyId_EstadoId",
            table: "Dtes",
            columns: new[] { "CompanyId", "EstadoId" });

        migrationBuilder.CreateIndex(
            name: "IX_Establishments_CompanyId",
            table: "Establishments",
            column: "CompanyId");

        migrationBuilder.CreateIndex(
            name: "IX_LibrosIva_CompanyId",
            table: "LibrosIva",
            column: "CompanyId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Dtes");

        migrationBuilder.DropTable(
            name: "Establishments");

        migrationBuilder.DropTable(
            name: "LibrosIva");

        migrationBuilder.DropTable(
            name: "Companies");
    }
}
