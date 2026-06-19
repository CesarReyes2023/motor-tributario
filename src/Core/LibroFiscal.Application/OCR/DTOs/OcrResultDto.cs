namespace LibroFiscal.Application.OCR.DTOs;

public record OcrResultDto(
    string RawText,
    string? NitEncontrado,
    string? NrcEncontrado,
    decimal? TotalEncontrado,
    decimal? IvaEncontrado,
    DateTimeOffset? FechaEncontrada,
    string? NumeroDocumento = null,
    string? NombreProveedor = null
);
