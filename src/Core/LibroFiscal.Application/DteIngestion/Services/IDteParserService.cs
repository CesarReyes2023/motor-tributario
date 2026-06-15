using LibroFiscal.Application.DteIngestion.DTOs;
using LibroFiscal.SharedKernel.Results;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LibroFiscal.Application.DteIngestion.Services;

/// <summary>
/// Interfaz unificada para extraer DTEs desde cualquier fuente de datos masiva (JSON, OCR, XML, etc.)
/// </summary>
public interface IDteParserService
{
    /// <summary>
    /// Intenta extraer uno o múltiples DTEs de un archivo en crudo.
    /// </summary>
    /// <param name="fileBytes">Los bytes del archivo (.json, .pdf, .jpg).</param>
    /// <param name="fileName">Nombre del archivo para determinar la extensión y lógica a aplicar.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Una lista de objetos DteExtractionDto extraídos con éxito.</returns>
    Task<Result<IEnumerable<DteExtractionDto>>> ParseAsync(byte[] fileBytes, string fileName, CancellationToken cancellationToken = default);
}
