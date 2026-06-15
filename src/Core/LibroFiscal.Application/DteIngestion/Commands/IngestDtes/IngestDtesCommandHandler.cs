using LibroFiscal.Application.Abstractions.Messaging;
using LibroFiscal.Application.DteIngestion.Services;
using LibroFiscal.SharedKernel.Results;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LibroFiscal.Application.DteIngestion.Commands.IngestDtes;

public sealed class IngestDtesCommandHandler : ICommandHandler<IngestDtesCommand, IngestionResultDto>
{
    private readonly IEnumerable<IDteParserService> _parsers;

    // TODO: Inyectar repositorios de Sales y Purchases para validación de duplicados.

    public IngestDtesCommandHandler(IEnumerable<IDteParserService> parsers)
    {
        _parsers = parsers;
    }

    public async Task<Result<IngestionResultDto>> Handle(IngestDtesCommand request, CancellationToken cancellationToken)
    {
        int processed = 0, inserted = 0, duplicates = 0, errors = 0;
        var errorMessages = new List<string>();

        foreach (var file in request.Files)
        {
            processed++;
            // 1. Elegir parser (por extensión, e.g. .json usa HaciendaJsonParserService, .jpg usa OcrScannerService)
            var parser = _parsers.FirstOrDefault(); // Por ahora toma el primero registrado (MVP)
            
            if (parser == null)
            {
                errors++;
                errorMessages.Add($"No hay un parser configurado para {file.FileName}");
                continue;
            }

            var parseResult = await parser.ParseAsync(file.FileBytes, file.FileName, cancellationToken);
            if (parseResult.IsFailure)
            {
                errors++;
                errorMessages.Add($"Error al parsear {file.FileName}: {parseResult.Error.Message}");
                continue;
            }

            // 2. Extraer los datos
            var extractedDtes = parseResult.Value;

            // 3. Traducir y Guardar
            foreach (var dte in extractedDtes)
            {
                // TODO: Usar HaciendaCatalogTranslator para determinar si es Compra o Venta
                // TODO: Validar duplicidad usando dte.CodigoGeneracion
                // TODO: Guardar en el Repositorio correspondiente

                inserted++; // Simulado para el MVP
            }
        }

        // await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new IngestionResultDto(processed, inserted, duplicates, errors, errorMessages));
    }
}
