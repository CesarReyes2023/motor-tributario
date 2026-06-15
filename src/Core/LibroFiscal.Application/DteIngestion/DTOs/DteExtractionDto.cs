using System;

namespace LibroFiscal.Application.DteIngestion.DTOs;

/// <summary>
/// Data Transfer Object estricto para los 10 campos clave extraídos de un DTE
/// (ya sea por archivo JSON del Ministerio o por escaneo OCR).
/// </summary>
public record DteExtractionDto(
    string NitEmisor,
    string NrcEmisor,
    DateTimeOffset FechaEmision,
    string TipoDocumento, // Código Hacienda, e.g. "03"
    decimal MontoTotal,
    decimal MontoIva,
    decimal VentasExentas,
    decimal VentasGravadas,
    string NumeroControl,
    string CodigoGeneracion
);
