using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LibroFiscal.Application.Abstractions.Messaging;
using LibroFiscal.Application.Abstractions.Services;
using LibroFiscal.Application.DteIngestion.Commands.IngestDtes;
using LibroFiscal.Application.DteIngestion.Services;
using LibroFiscal.Application.Purchases.Commands.RegisterPurchase;
using LibroFiscal.Domain.Common.Ids;
using LibroFiscal.Domain.Companies.Entities;
using LibroFiscal.SharedKernel.Interfaces;
using LibroFiscal.SharedKernel.Results;
using MediatR;

namespace LibroFiscal.Application.DteIngestion.Commands.ImportSelectedDtes;

public sealed class ImportSelectedDtesCommandHandler : ICommandHandler<ImportSelectedDtesCommand, IngestionResultDto>
{
    private readonly IRepository<Company, CompanyId> _companyRepository;
    private readonly IHaciendaService _haciendaService;
    private readonly IEnumerable<IDteParserService> _parsers;
    private readonly IMediator _mediator;
    private readonly IRepository<LibroFiscal.Domain.Purchases.Entities.Purchase, LibroFiscal.Domain.Common.Ids.PurchaseId> _purchaseRepository;
    private readonly IRepository<LibroFiscal.Domain.Sales.Entities.Sale, LibroFiscal.Domain.Common.Ids.SaleId> _saleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ImportSelectedDtesCommandHandler(
        IRepository<Company, CompanyId> companyRepository,
        IHaciendaService haciendaService,
        IEnumerable<IDteParserService> parsers,
        IMediator mediator,
        IRepository<LibroFiscal.Domain.Purchases.Entities.Purchase, LibroFiscal.Domain.Common.Ids.PurchaseId> purchaseRepository,
        IRepository<LibroFiscal.Domain.Sales.Entities.Sale, LibroFiscal.Domain.Common.Ids.SaleId> saleRepository,
        IUnitOfWork unitOfWork)
    {
        _companyRepository = companyRepository;
        _haciendaService = haciendaService;
        _parsers = parsers;
        _mediator = mediator;
        _purchaseRepository = purchaseRepository;
        _saleRepository = saleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IngestionResultDto>> Handle(ImportSelectedDtesCommand request, CancellationToken cancellationToken)
    {
        var company = await _companyRepository.GetByIdAsync(CompanyId.From(request.CompanyId), cancellationToken);
        if (company == null)
            return Result.Failure<IngestionResultDto>(Error.NotFound("Company.NotFound", "Empresa no encontrada."));

        if (string.IsNullOrWhiteSpace(company.ApiPassword))
            return Result.Failure<IngestionResultDto>(Error.Validation("Company.NoApiPassword", "La empresa no tiene configurada su Clave API de Hacienda."));

        // 1. Autenticar
        var authResult = await _haciendaService.AuthenticateAsync(request.CompanyId, cancellationToken);
        if (authResult.IsFailure)
            return Result.Failure<IngestionResultDto>(authResult.Error);

        var token = authResult.Value.Token;

        int processed = 0, inserted = 0, duplicates = 0, errors = 0;
        var errorMessages = new List<string>();

        var jsonParser = _parsers.FirstOrDefault(p => p.GetType().Name.Contains("Json"));
        if (jsonParser == null)
            return Result.Failure<IngestionResultDto>(Error.Failure("Parser.NotFound", "No hay un parser de JSON configurado."));

        var purchasesToInsert = new List<LibroFiscal.Domain.Purchases.Entities.Purchase>();
        var salesToInsert = new List<LibroFiscal.Domain.Sales.Entities.Sale>();

        // Limpiar el NIT de la empresa (sin guiones)
        var companyNitClean = company.Nit.Value.Replace("-", "");

        // 2. Descargar cada DTE seleccionado
        foreach (var dto in request.SelectedDtes)
        {
            processed++;
            var jsonRes = await _haciendaService.DownloadDteJsonAsync(token, dto.CodigoGeneracion, cancellationToken);
            if (jsonRes.IsFailure)
            {
                errors++;
                errorMessages.Add($"Error al descargar DTE {dto.NumeroControl}: {jsonRes.Error.Message}");
                continue;
            }

            var bytes = Encoding.UTF8.GetBytes(jsonRes.Value);
            var fileName = $"ApiSync_DTE_{dto.CodigoGeneracion}.json";

            var parseResult = await jsonParser.ParseAsync(bytes, fileName, cancellationToken);
            if (parseResult.IsFailure)
            {
                errors++;
                errorMessages.Add($"Error al parsear documento de la API ({dto.NumeroControl}): {parseResult.Error.Message}");
                continue;
            }

            var extractedDtes = parseResult.Value;
            foreach (var dte in extractedDtes)
            {
                // Determinar si es emitido (Venta) o recibido (Compra)
                var emisorNitClean = (dte.NitEmisor ?? "").Replace("-", "");
                bool isSale = emisorNitClean == companyNitClean;

                if (isSale)
                {
                    // Es un DTE Emitido por nosotros -> Se registra en Ventas
                    var saleResult = LibroFiscal.Domain.Sales.Entities.Sale.Create(
                        company.Id,
                        dte.NitEmisor ?? "",
                        dte.NrcEmisor ?? "",
                        "Cliente " + (dte.NitEmisor ?? "Consumidor"), // Provisional
                        dte.FechaEmision.DateTime,
                        dte.NumeroControl,
                        dte.VentasGravadas + dte.VentasExentas,
                        dte.VentasExentas,
                        dte.MontoIva,
                        dte.MontoTotal);

                    if (saleResult.IsSuccess)
                    {
                        salesToInsert.Add(saleResult.Value);
                        inserted++;
                    }
                    else
                    {
                        if (saleResult.Error.Code.Contains("Duplicate", StringComparison.OrdinalIgnoreCase))
                            duplicates++;
                        else
                        {
                            errors++;
                            errorMessages.Add($"Error al guardar venta sincronizada {dte.NumeroControl}: {saleResult.Error.Message}");
                        }
                    }
                }
                else
                {
                    // Es un DTE Recibido -> Se registra en Compras
                    var purchaseResult = LibroFiscal.Domain.Purchases.Entities.Purchase.Create(
                        company.Id,
                        dte.NitEmisor ?? "",
                        dte.NrcEmisor ?? "",
                        string.IsNullOrEmpty(dte.NrcEmisor) ? "Proveedor" : "Proveedor " + (dte.NitEmisor ?? ""),
                        dte.FechaEmision.DateTime,
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
                        if (purchaseResult.Error.Code.Contains("Duplicate", StringComparison.OrdinalIgnoreCase))
                            duplicates++;
                        else
                        {
                            errors++;
                            errorMessages.Add($"Error al guardar compra sincronizada {dte.NumeroControl}: {purchaseResult.Error.Message}");
                        }
                    }
                }
            }
        }
        
        if (purchasesToInsert.Count > 0)
        {
            _purchaseRepository.AddRange(purchasesToInsert);
        }
        
        if (salesToInsert.Count > 0)
        {
            _saleRepository.AddRange(salesToInsert);
        }

        if (purchasesToInsert.Count > 0 || salesToInsert.Count > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result.Success(new IngestionResultDto(processed, inserted, duplicates, errors, errorMessages));
    }
}
