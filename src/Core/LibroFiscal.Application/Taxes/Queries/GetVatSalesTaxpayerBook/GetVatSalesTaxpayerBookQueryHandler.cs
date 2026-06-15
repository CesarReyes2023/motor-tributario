using LibroFiscal.Application.Abstractions.Messaging;
using LibroFiscal.Domain.Common.Ids;
using LibroFiscal.Domain.Common.ValueObjects;
using LibroFiscal.Domain.DTE.Entities;
using LibroFiscal.SharedKernel.Interfaces;
using LibroFiscal.SharedKernel.Results;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LibroFiscal.Application.Taxes.Queries.GetVatSalesTaxpayerBook;

internal sealed class GetVatSalesTaxpayerBookQueryHandler : IQueryHandler<GetVatSalesTaxpayerBookQuery, IReadOnlyList<VatSalesTaxpayerDto>>
{
    private readonly IRepository<DteDocument, DteId> _dteRepository;

    public GetVatSalesTaxpayerBookQueryHandler(IRepository<DteDocument, DteId> dteRepository)
    {
        _dteRepository = dteRepository;
    }

    public async Task<Result<IReadOnlyList<VatSalesTaxpayerDto>>> Handle(
        GetVatSalesTaxpayerBookQuery request,
        CancellationToken cancellationToken)
    {
        var periodResult = FiscalPeriod.Create(request.Year, request.Month);
        if (periodResult.IsFailure)
        {
            return Result.Failure<IReadOnlyList<VatSalesTaxpayerDto>>(periodResult.Error);
        }

        FiscalPeriod period = periodResult.Value;

        // Fetch DTEs of type "03" (CCF)
        var dtes = await _dteRepository.FindAsync(
            d => d.TipoDte.Codigo == "03" &&
                 d.FechaEmision >= period.StartDate &&
                 d.FechaEmision <= period.EndDate,
            cancellationToken);

        var sales = dtes
            .OrderBy(d => d.FechaEmision)
            .ThenBy(d => d.NumeroControl.Value)
            .Select(d => new VatSalesTaxpayerDto
            {
                EmisionDate = d.FechaEmision,
                DocumentNumber = d.NumeroControl.Value,
                NrcCustomer = d.Receptor != null && d.Receptor.Nrc != null ? d.Receptor.Nrc : string.Empty,
                CustomerName = d.Receptor != null ? d.Receptor.Nombre : string.Empty,
                ExemptSales = d.Resumen.TotalExenta,
                LocalGravadaSales = d.Resumen.TotalGravada,
                FiscalDebit = d.Resumen.TotalIva,
                RetainedIva = 0m, 
                TotalSales = d.Resumen.TotalPagar
            })
            .ToList();

        return sales.AsReadOnly();
    }
}
