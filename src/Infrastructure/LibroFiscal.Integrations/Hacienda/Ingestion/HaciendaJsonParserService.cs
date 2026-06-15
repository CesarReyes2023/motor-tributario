using LibroFiscal.Application.DteIngestion.DTOs;
using LibroFiscal.Application.DteIngestion.Services;
using LibroFiscal.SharedKernel.Results;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace LibroFiscal.Integrations.Hacienda.Ingestion;

public class HaciendaJsonParserService : IDteParserService
{
    public Task<Result<IEnumerable<DteExtractionDto>>> ParseAsync(byte[] fileBytes, string fileName, CancellationToken cancellationToken = default)
    {
        try
        {
            var jsonString = System.Text.Encoding.UTF8.GetString(fileBytes);
            var document = JsonDocument.Parse(jsonString);

            var extractedDtes = new List<DteExtractionDto>();

            // El JSON de Hacienda usualmente tiene la raíz o un array.
            // Para el MVP asumimos que parsearemos la estructura básica del DTE.
            var root = document.RootElement;
            
            // Lógica de navegación del JSON de Hacienda (identificacion, resumen, receptor, etc.)
            // Estos campos deben extraerse de los nodos específicos del JSON estructurado del MH.
            
            // Placeholder: Parsear el documento individual
            if (root.TryGetProperty("identificacion", out var identificacion) &&
                root.TryGetProperty("resumen", out var resumen) &&
                root.TryGetProperty("receptor", out var receptor))
            {
                var tipoDoc = identificacion.GetProperty("tipoDte").GetString() ?? "";
                var codGeneracion = identificacion.GetProperty("codigoGeneracion").GetString() ?? "";
                var numControl = identificacion.GetProperty("numeroControl").GetString() ?? "";
                var fechaStr = identificacion.GetProperty("fecEmi").GetString() ?? "";
                
                _ = DateTimeOffset.TryParse(fechaStr, out var fechaEmision);

                var nitReceptor = receptor.TryGetProperty("nit", out var n) ? n.GetString() : "";
                var nrcReceptor = receptor.TryGetProperty("nrc", out var r) ? r.GetString() : "";

                var totalPagar = resumen.GetProperty("totalPagar").GetDecimal();
                var totalIva = resumen.TryGetProperty("totalIva", out var iva) ? iva.GetDecimal() : 0m;
                var totalExentas = resumen.TryGetProperty("totalExenta", out var ex) ? ex.GetDecimal() : 0m;
                var totalGravadas = resumen.TryGetProperty("totalGravada", out var gr) ? gr.GetDecimal() : 0m;

                var dto = new DteExtractionDto(
                    NitEmisor: nitReceptor ?? "", // En una compra, el "emisor" del DTE es nuestro proveedor. Si es una venta, somos nosotros. Depende de la perspectiva.
                    NrcEmisor: nrcReceptor ?? "",
                    FechaEmision: fechaEmision,
                    TipoDocumento: tipoDoc,
                    MontoTotal: totalPagar,
                    MontoIva: totalIva,
                    VentasExentas: totalExentas,
                    VentasGravadas: totalGravadas,
                    NumeroControl: numControl,
                    CodigoGeneracion: codGeneracion
                );

                extractedDtes.Add(dto);
            }

            return Task.FromResult(Result.Success<IEnumerable<DteExtractionDto>>(extractedDtes));
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result.Failure<IEnumerable<DteExtractionDto>>(Error.Failure("JsonParser.Error", $"Error al leer JSON de Hacienda: {ex.Message}")));
        }
    }
}
