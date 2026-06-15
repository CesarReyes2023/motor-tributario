using LibroFiscal.Application.Abstractions.Messaging;
using LibroFiscal.Domain.Common.Ids;
using LibroFiscal.Domain.Common.ValueObjects;
using LibroFiscal.Domain.DTE.Entities;
using LibroFiscal.SharedKernel.Interfaces;
using LibroFiscal.SharedKernel.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LibroFiscal.Application.Taxes.Queries.GetVatSalesConsumerBook;

internal sealed class GetVatSalesConsumerBookQueryHandler : IQueryHandler<GetVatSalesConsumerBookQuery, IReadOnlyList<VatSalesConsumerDto>>
{
    private readonly IRepository<DteDocument, DteId> _dteRepository;

    public GetVatSalesConsumerBookQueryHandler(IRepository<DteDocument, DteId> dteRepository)
    {
        _dteRepository = dteRepository;
    }

    public async Task<Result<IReadOnlyList<VatSalesConsumerDto>>> Handle(
        GetVatSalesConsumerBookQuery request,
        CancellationToken cancellationToken)
    {
        var periodResult = FiscalPeriod.Create(request.Year, request.Month);
        if (periodResult.IsFailure)
        {
            return Result.Failure<IReadOnlyList<VatSalesConsumerDto>>(periodResult.Error);
        }

        FiscalPeriod period = periodResult.Value;

        // Fetch DTEs of type "01" (Factura) and "11" (Factura Exportacion - if we add it)
        var dtes = await _dteRepository.FindAsync(
            d => (d.TipoDte.Codigo == "01" || d.TipoDte.Codigo == "11") &&
                 d.FechaEmision >= period.StartDate &&
                 d.FechaEmision <= period.EndDate,
            cancellationToken);

        var sales = dtes
            .OrderBy(d => d.FechaEmision)
            .ThenBy(d => d.NumeroControl.Value)
            .ToList();

        // Group by day to generate the report as it is usually required for Consumer Sales
        var groupedSales = sales
            .GroupBy(d => d.FechaEmision.Date)
            .Select(g => new VatSalesConsumerDto
            {
                EmisionDate = new DateTimeOffset(g.Key, TimeSpan.Zero),
                InitialDocumentNumber = g.First().NumeroControl.Value,
                FinalDocumentNumber = g.Last().NumeroControl.Value,
                ExemptSales = g.Sum(d => d.Resumen.TotalExenta),
                LocalGravadaSales = g.Sum(d => d.Resumen.TotalGravada),
                ExportSales = g.Where(d => d.TipoDte.Codigo == "11").Sum(d => d.Resumen.TotalGravada),
                TotalSales = g.Sum(d => d.Resumen.TotalPagar),
                RetainedIva = 0m
            })
            .OrderBy(dto => dto.EmisionDate)
            .ToList();

        return groupedSales.AsReadOnly();
    }
}
