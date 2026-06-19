using LibroFiscal.Application.Abstractions.Messaging;
using LibroFiscal.Domain.Common.Ids;
using LibroFiscal.Domain.Common.ValueObjects;
using LibroFiscal.Domain.DTE.Entities;
using LibroFiscal.Domain.Purchases.Entities;
using LibroFiscal.SharedKernel.Interfaces;
using LibroFiscal.SharedKernel.Results;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LibroFiscal.Domain.Common.Enumerations;

namespace LibroFiscal.Application.Taxes.Queries.GetDashboardMetrics;

internal sealed class GetDashboardMetricsQueryHandler : IQueryHandler<GetDashboardMetricsQuery, DashboardMetricsDto>
{
    private readonly IRepository<DteDocument, DteId> _dteRepository;
    private readonly IRepository<Purchase, PurchaseId> _purchaseRepository;

    public GetDashboardMetricsQueryHandler(
        IRepository<DteDocument, DteId> dteRepository,
        IRepository<Purchase, PurchaseId> purchaseRepository)
    {
        _dteRepository = dteRepository;
        _purchaseRepository = purchaseRepository;
    }

    public async Task<Result<DashboardMetricsDto>> Handle(
        GetDashboardMetricsQuery request,
        CancellationToken cancellationToken)
    {
        var periodResult = FiscalPeriod.Create(request.Year, request.Month);
        if (periodResult.IsFailure)
        {
            return Result.Failure<DashboardMetricsDto>(periodResult.Error);
        }

        FiscalPeriod period = periodResult.Value;

        // Fetch Sales (DTEs)
        var dtes = await _dteRepository.FindAsync(
            d => d.CompanyId.Value == request.CompanyId,
            cancellationToken);

        // Fetch Purchases
        var purchases = await _purchaseRepository.FindAsync(
            p => p.CompanyId.Value == request.CompanyId, 
            cancellationToken);

        // Calculate Sales Metrics
        var sales = dtes.Where(d => d.TipoDte.Codigo == "01" || d.TipoDte.Codigo == "03" || d.TipoDte.Codigo == "11").ToList();
        
        decimal totalSales = sales.Sum(d => d.Resumen.TotalGravada + d.Resumen.TotalExenta);
        decimal totalSalesTaxes = sales.Where(d => d.TipoDte.Codigo == "03").Sum(d => d.Resumen.TotalIva);

        // Pending DTEs (Borrador, ErrorFinal, Rechazado)
        int pendingDtes = dtes.Count(d => d.Estado == EstadoDte.Borrador || 
                                          d.Estado == EstadoDte.Rechazado || 
                                          d.Estado == EstadoDte.ErrorFinal);

        // Calculate Purchase Metrics
        decimal totalPurchases = purchases.Sum(p => p.SubTotal);
        decimal totalPurchaseTaxes = purchases.Sum(p => p.TaxAmount);

        return new DashboardMetricsDto
        {
            TotalSales = totalSales,
            TotalSalesTaxes = totalSalesTaxes,
            TotalPurchases = totalPurchases,
            TotalPurchaseTaxes = totalPurchaseTaxes,
            PendingDtesCount = pendingDtes
        };
    }
}
