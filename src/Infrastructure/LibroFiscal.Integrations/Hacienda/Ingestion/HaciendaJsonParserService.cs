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
            
            var elementsToProcess = new List<JsonElement>();
            if (root.ValueKind == JsonValueKind.Array)
            {
                elementsToProcess.AddRange(root.EnumerateArray());
            }
            else
            {
                elementsToProcess.Add(root);
            }

            foreach (var element in elementsToProcess)
            {
                var dteRoot = element;
                
                // A veces el MH anida el DTE dentro de un nodo "dteJson" o "documento"
                if (dteRoot.TryGetProperty("dteJson", out var dteJsonProp))
                {
                    dteRoot = dteJsonProp;
                }
                else if (dteRoot.TryGetProperty("documento", out var docProp))
                {
                    dteRoot = docProp;
                }

                if (dteRoot.TryGetProperty("identificacion", out var identificacion) &&
                    dteRoot.TryGetProperty("resumen", out var resumen))
                {
                    var tipoDoc = identificacion.TryGetProperty("tipoDte", out var t) ? t.GetString() : "";
                    var codGeneracion = identificacion.TryGetProperty("codigoGeneracion", out var cg) ? cg.GetString() : "";
                    var numControl = identificacion.TryGetProperty("numeroControl", out var nc) ? nc.GetString() : "";
                    var fechaStr = identificacion.TryGetProperty("fecEmi", out var fe) ? fe.GetString() : "";
                    
                    _ = DateTimeOffset.TryParse(fechaStr, out var fechaEmision);

                    // Emisor o Receptor dependiendo si es factura recibida o emitida
                    var emisor = dteRoot.TryGetProperty("emisor", out var em) ? em : default;
                    var nitEmisor = emisor.ValueKind != JsonValueKind.Undefined && emisor.TryGetProperty("nit", out var nEmisor) ? nEmisor.GetString() : "";
                    var nrcEmisor = emisor.ValueKind != JsonValueKind.Undefined && emisor.TryGetProperty("nrc", out var rEmisor) ? rEmisor.GetString() : "";

                    // Intenta recuperar el receptor si el emisor falla
                    if (string.IsNullOrEmpty(nitEmisor) && dteRoot.TryGetProperty("receptor", out var receptor))
                    {
                        nitEmisor = receptor.TryGetProperty("nit", out var nRec) ? nRec.GetString() : "";
                        nrcEmisor = receptor.TryGetProperty("nrc", out var rRec) ? rRec.GetString() : "";
                    }

                    var totalPagar = resumen.TryGetProperty("totalPagar", out var tp) ? tp.GetDecimal() : 
                                     (resumen.TryGetProperty("montoTotalOperacion", out var mt) ? mt.GetDecimal() : 0m);
                                     
                    var totalIva = resumen.TryGetProperty("totalIva", out var iva) ? iva.GetDecimal() : 0m;
                    var totalExentas = resumen.TryGetProperty("totalExenta", out var ex) ? ex.GetDecimal() : 0m;
                    var totalGravadas = resumen.TryGetProperty("totalGravada", out var gr) ? gr.GetDecimal() : 0m;

                    var dto = new DteExtractionDto(
                        NitEmisor: nitEmisor ?? "", 
                        NrcEmisor: nrcEmisor ?? "",
                        FechaEmision: fechaEmision,
                        TipoDocumento: tipoDoc ?? "",
                        MontoTotal: totalPagar,
                        MontoIva: totalIva,
                        VentasExentas: totalExentas,
                        VentasGravadas: totalGravadas,
                        NumeroControl: numControl ?? "",
                        CodigoGeneracion: codGeneracion ?? ""
                    );

                    extractedDtes.Add(dto);
                }
                else if (dteRoot.TryGetProperty("fechaEmision", out var fechaEmisionProp) &&
                         dteRoot.TryGetProperty("numeroDocumento", out var numeroDocProp))
                {
                    // Soporte para JSON simplificado/mock
                    _ = DateTimeOffset.TryParse(fechaEmisionProp.GetString(), out var fechaEmision);
                    
                    var nitEmisor = dteRoot.TryGetProperty("nitProveedor", out var np) ? np.GetString() : "";
                    var nrcEmisor = dteRoot.TryGetProperty("nrcProveedor", out var nrcp) ? nrcp.GetString() : "";
                    var numControl = numeroDocProp.GetString() ?? "";
                    
                    var totalPagar = dteRoot.TryGetProperty("total", out var t) ? t.GetDecimal() : 0m;
                    var totalIva = dteRoot.TryGetProperty("creditoFiscal", out var cf) ? cf.GetDecimal() : 0m;
                    var totalExentas = dteRoot.TryGetProperty("comprasExentas", out var ce) ? ce.GetDecimal() : 0m;
                    var totalGravadas = dteRoot.TryGetProperty("comprasGravadas", out var cg) ? cg.GetDecimal() : 0m;

                    var dto = new DteExtractionDto(
                        NitEmisor: nitEmisor ?? "", 
                        NrcEmisor: nrcEmisor ?? "",
                        FechaEmision: fechaEmision,
                        TipoDocumento: "03", // Asumimos CCF para el mock
                        MontoTotal: totalPagar,
                        MontoIva: totalIva,
                        VentasExentas: totalExentas,
                        VentasGravadas: totalGravadas,
                        NumeroControl: numControl,
                        CodigoGeneracion: System.Guid.NewGuid().ToString().ToUpperInvariant() // Mock UUID
                    );

                    extractedDtes.Add(dto);
                }
                else
                {
                    // Opcional: registrar que un elemento falló. Para el MVP ignoramos elementos vacíos en el array o fallamos.
                    if (elementsToProcess.Count == 1) 
                    {
                        var keys = dteRoot.ValueKind == JsonValueKind.Object ? string.Join(", ", System.Linq.Enumerable.Select(dteRoot.EnumerateObject(), x => x.Name)) : dteRoot.ValueKind.ToString();
                        return Task.FromResult(Result.Failure<IEnumerable<DteExtractionDto>>(Error.Failure("JsonParser.Format", $"Estructura de DTE desconocida. Nodos encontrados: {keys}")));
                    }
                }
            }

            return Task.FromResult(Result.Success<IEnumerable<DteExtractionDto>>(extractedDtes));
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result.Failure<IEnumerable<DteExtractionDto>>(Error.Failure("JsonParser.Error", $"Error al leer JSON de Hacienda: {ex.Message}")));
        }
    }
}
