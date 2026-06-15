using LibroFiscal.Application.Abstractions.Messaging;
using System.Collections.Generic;

namespace LibroFiscal.Application.DteIngestion.Commands.IngestDtes;

/// <summary>
/// Archivo crudo (ruta o bytes) para ser procesado por el motor de Ingesta Masiva.
/// </summary>
public record RawIngestionFile(string FileName, byte[] FileBytes);

public sealed record IngestDtesCommand(IEnumerable<RawIngestionFile> Files) : ICommand<IngestionResultDto>;

public record IngestionResultDto(
    int TotalProcessed,
    int TotalInserted,
    int TotalDuplicates,
    int TotalErrors,
    IEnumerable<string> ErrorMessages
);
