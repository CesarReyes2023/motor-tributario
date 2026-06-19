using LibroFiscal.Application.Abstractions.Messaging;
using MediatR;
using LibroFiscal.Application.DteIngestion.Services;
using LibroFiscal.Application.Purchases.Commands.RegisterPurchase;
using LibroFiscal.Domain.Common.Ids;
using LibroFiscal.Domain.Companies.Entities;
using LibroFiscal.SharedKernel.Interfaces;
using LibroFiscal.SharedKernel.Results;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LibroFiscal.Application.DteIngestion.Commands.IngestDtes;

public sealed class IngestDtesCommandHandler : ICommandHandler<IngestDtesCommand, IngestionResultDto>
{
    private readonly IEnumerable<IDteParserService> _parsers;
    private readonly IMediator _mediator;
    private readonly IRepository<Company, CompanyId> _companyRepository;
    private readonly IRepository<LibroFiscal.Domain.Purchases.Entities.Purchase, LibroFiscal.Domain.Common.Ids.PurchaseId> _purchaseRepository;

    public IngestDtesCommandHandler(
        IEnumerable<IDteParserService> parsers,
        IMediator mediator,
        IRepository<Company, CompanyId> companyRepository,
        IRepository<LibroFiscal.Domain.Purchases.Entities.Purchase, LibroFiscal.Domain.Common.Ids.PurchaseId> purchaseRepository)
    {
        _parsers = parsers;
        _mediator = mediator;
        _companyRepository = companyRepository;
        _purchaseRepository = purchaseRepository;
    }

    public async Task<Result<IngestionResultDto>> Handle(IngestDtesCommand request, CancellationToken cancellationToken)
    {
        int processed = 0, inserted = 0, duplicates = 0, errors = 0;
        var errorMessages = new List<string>();

        // Utilizar la CompanyId inyectada desde la UI
        var company = await _companyRepository.GetByIdAsync(new CompanyId(request.CompanyId), cancellationToken);

        if (company == null)
        {
            return Result.Failure<IngestionResultDto>(Error.Validation("Company.NotFound", "La empresa activa no existe o no se pudo cargar."));
        }

        foreach (var file in request.Files)
        {
            processed++;
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

            var extractedDtes = parseResult.Value;
            var purchasesToInsert = new List<LibroFiscal.Domain.Purchases.Entities.Purchase>();

            foreach (var dte in extractedDtes)
            {
                // Asumimos que todos los DTEs ingresados por esta vía masiva (portal recibidos) son compras
                var purchaseResult = LibroFiscal.Domain.Purchases.Entities.Purchase.Create(
                    company.Id,
                    dte.NitEmisor,
                    dte.NrcEmisor,
                    string.IsNullOrEmpty(dte.NrcEmisor) ? "Proveedor" : "Proveedor " + dte.NitEmisor,
                    dte.FechaEmision.UtcDateTime,
                    dte.NumeroControl,
                    dte.VentasGravadas + dte.VentasExentas,
                    dte.MontoIva,
                    dte.MontoTotal);

                if (purchaseResult.IsSuccess)
                {
                    purchasesToInsert.Add(purchaseResult.Value);
                    inserted++;
                }
                else
                {
                    errors++;
                    errorMessages.Add($"Error al crear entidad de compra {dte.NumeroControl}: {purchaseResult.Error.Message}");
                }
            }
            
            if (purchasesToInsert.Count > 0)
            {
                _purchaseRepository.AddRange(purchasesToInsert);
            }
        }

        return Result.Success(new IngestionResultDto(processed, inserted, duplicates, errors, errorMessages));
    }
}
