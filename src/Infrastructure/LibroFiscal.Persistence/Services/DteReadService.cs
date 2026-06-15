using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LibroFiscal.Application.DTE.Queries.GetDtes;
using Microsoft.EntityFrameworkCore;

namespace LibroFiscal.Persistence.Services;

public sealed class DteReadService : IDteReadService
{
    private readonly LibroFiscalDbContext _dbContext;

    public DteReadService(LibroFiscalDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<DteSummaryDto>> GetDtesAsync(CancellationToken cancellationToken = default)
    {
        var dtos = new List<DteSummaryDto>();
        var connection = _dbContext.Database.GetDbConnection();
        var wasClosed = connection.State == System.Data.ConnectionState.Closed;
        if (wasClosed) await connection.OpenAsync(cancellationToken);

        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT ""Id"", ""NumeroControl"", ""Receptor"", ""Resumen"", ""EstadoId"", ""SelloRecepcion"", ""FechaEmision""
                FROM ""Dtes""
                ORDER BY ""FechaEmision"" DESC";

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var id = reader.GetGuid(0);
                var numeroControl = reader.GetString(1);
                
                var receptorJson = reader.IsDBNull(2) ? null : reader.GetString(2);
                string cliente = "Consumidor Final";
                if (!string.IsNullOrWhiteSpace(receptorJson))
                {
                    try {
                        using var doc = System.Text.Json.JsonDocument.Parse(receptorJson);
                        if (doc.RootElement.TryGetProperty("Nombre", out var nombreProp) || doc.RootElement.TryGetProperty("nombre", out nombreProp))
                            cliente = nombreProp.GetString() ?? "Consumidor Final";
                    } catch { }
                }

                var resumenJson = reader.IsDBNull(3) ? null : reader.GetString(3);
                decimal totalPagar = 0;
                
                if (!string.IsNullOrWhiteSpace(resumenJson))
                {
                    try {
                        using var doc = System.Text.Json.JsonDocument.Parse(resumenJson);
                        foreach (var prop in doc.RootElement.EnumerateObject())
                        {
                            if (string.Equals(prop.Name, "TotalPagar", StringComparison.OrdinalIgnoreCase) || 
                                string.Equals(prop.Name, "totalPagar", StringComparison.OrdinalIgnoreCase))
                            {
                                if (prop.Value.ValueKind == System.Text.Json.JsonValueKind.String)
                                {
                                    if (decimal.TryParse(prop.Value.GetString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var val))
                                        totalPagar = val;
                                }
                                else if (prop.Value.ValueKind == System.Text.Json.JsonValueKind.Number)
                                {
                                    totalPagar = prop.Value.GetDecimal();
                                }
                                break;
                            }
                        }
                    } catch { }
                }

                var estadoId = reader.GetInt32(4);
                var estado = estadoId switch {
                    1 => "Borrador",
                    2 => "Validado",
                    3 => "Firmado",
                    4 => "EnCola",
                    5 => "Transmitiendo",
                    6 => "Sellado",
                    7 => "Rechazado",
                    8 => "ErrorFinal",
                    9 => "Anulado",
                    _ => "Desconocido"
                };

                var selloRecepcion = reader.IsDBNull(5) ? null : reader.GetString(5);
                
                // Fetch timestamp directly
                var fechaEmision = reader.GetFieldValue<DateTimeOffset>(6);

                dtos.Add(new DteSummaryDto(
                    id,
                    numeroControl,
                    "Factura",
                    cliente,
                    totalPagar,
                    estado,
                    selloRecepcion,
                    fechaEmision
                ));
            }
        }
        finally
        {
            if (wasClosed) await connection.CloseAsync();
        }

        return dtos;
    }
}
