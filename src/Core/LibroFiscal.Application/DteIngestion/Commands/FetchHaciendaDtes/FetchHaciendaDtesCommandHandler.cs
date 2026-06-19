using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LibroFiscal.Application.Abstractions.Messaging;
using LibroFiscal.Application.Abstractions.Services;
using LibroFiscal.Domain.Common.Ids;
using LibroFiscal.Domain.Companies.Entities;
using LibroFiscal.SharedKernel.Interfaces;
using LibroFiscal.SharedKernel.Results;

namespace LibroFiscal.Application.DteIngestion.Commands.FetchHaciendaDtes;

public sealed class FetchHaciendaDtesCommandHandler : ICommandHandler<FetchHaciendaDtesCommand, List<DteDownloadDto>>
{
    private readonly IRepository<Company, CompanyId> _companyRepository;
    private readonly IHaciendaService _haciendaService;

    public FetchHaciendaDtesCommandHandler(
        IRepository<Company, CompanyId> companyRepository,
        IHaciendaService haciendaService)
    {
        _companyRepository = companyRepository;
        _haciendaService = haciendaService;
    }

    public async Task<Result<List<DteDownloadDto>>> Handle(FetchHaciendaDtesCommand request, CancellationToken cancellationToken)
    {
        var company = await _companyRepository.GetByIdAsync(CompanyId.From(request.CompanyId), cancellationToken);
        if (company == null)
            return Result.Failure<List<DteDownloadDto>>(Error.NotFound("Company.NotFound", "Empresa no encontrada."));

        if (string.IsNullOrWhiteSpace(company.ApiPassword))
            return Result.Failure<List<DteDownloadDto>>(Error.Validation("Company.NoApiPassword", "La empresa no tiene configurada su Clave API de Hacienda."));

        // 1. Autenticar
        var authResult = await _haciendaService.AuthenticateAsync(request.CompanyId, cancellationToken);
        if (authResult.IsFailure)
            return Result.Failure<List<DteDownloadDto>>(authResult.Error);

        var token = authResult.Value.Token;

        // 2. Consultar DTEs Recibidos (Metadatos)
        var metadataResult = await _haciendaService.QueryReceivedDtesAsync(token, company.Nit.Value, request.StartDate, request.EndDate, cancellationToken);
        if (metadataResult.IsFailure)
            return Result.Failure<List<DteDownloadDto>>(metadataResult.Error);

        var dtos = metadataResult.Value.Select(m => new DteDownloadDto(
            m.CodigoGeneracion,
            m.NumeroControl,
            m.FechaEmision.DateTime,
            m.TipoDte,
            m.NombreEmisor,
            m.MontoTotal,
            m.SelloRecepcion
        )).ToList();

        return Result.Success(dtos);
    }
}
