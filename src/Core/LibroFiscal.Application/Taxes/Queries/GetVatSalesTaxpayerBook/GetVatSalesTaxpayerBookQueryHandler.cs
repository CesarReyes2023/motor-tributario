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

        // Obtener ventas del mes y año especificado
        var startDate = new System.DateTimeOffset(new System.DateTime(request.Year, request.Month, 1), System.TimeSpan.Zero);
        var endDate = startDate.AddMonths(1);

        var companyId = new CompanyId(request.CompanyId);

        // Fetch DTEs of type "03" (CCF)
        var dtes = await _dteRepository.FindAsync(
            d => d.CompanyId == companyId &&
                 d.TipoDte.Codigo == "03" &&
                 d.FechaEmision >= startDate &&
                 d.FechaEmision < endDate,
            q => q.OrderBy(d => d.FechaEmision).ThenBy(d => d.NumeroControl),
            cancellationToken);

        var sales = dtes
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
