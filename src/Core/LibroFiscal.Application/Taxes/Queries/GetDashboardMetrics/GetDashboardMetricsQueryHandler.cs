using LibroFiscal.Application.Abstractions.Messaging;
using LibroFiscal.Domain.Common.Ids;
using LibroFiscal.Domain.Common.ValueObjects;
using LibroFiscal.Domain.DTE.Entities;
using LibroFiscal.Domain.Purchases.Entities;
using LibroFiscal.Domain.Sales.Entities;
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
    private readonly IRepository<Sale, SaleId> _saleRepository;

    public GetDashboardMetricsQueryHandler(
        IRepository<DteDocument, DteId> dteRepository,
        IRepository<Purchase, PurchaseId> purchaseRepository,
        IRepository<Sale, SaleId> saleRepository)
    {
        _dteRepository = dteRepository;
        _purchaseRepository = purchaseRepository;
        _saleRepository = saleRepository;
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

        // PONITAIL: El dashboard debe filtrar estrictamente por el periodo solicitado.
        var startDate = new System.DateTime(request.Year, request.Month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        // Fetch DTEs
        var dtes = await _dteRepository.FindAsync(
            d => d.CompanyId.Value == request.CompanyId,
            cancellationToken);

        // Fetch Purchases for current period only
        var purchases = await _purchaseRepository.FindAsync(
            p => p.CompanyId.Value == request.CompanyId, 
            cancellationToken);

        // Fetch Manual Sales for current period
        var manualSales = await _saleRepository.FindAsync(
            s => s.CompanyId.Value == request.CompanyId && s.IssueDate.Year == request.Year && s.IssueDate.Month == request.Month,
            cancellationToken);

        // Calculate Sales Metrics (DTEs in period + Manual Sales in period)
        var dtesInPeriod = dtes.Where(d => d.FechaEmision.Year == request.Year && d.FechaEmision.Month == request.Month).ToList();

        var validDtesInPeriod = dtesInPeriod.Where(d => d.TipoDte.Codigo == "01" || d.TipoDte.Codigo == "03" || d.TipoDte.Codigo == "11").ToList();
        
        decimal totalDteSales = validDtesInPeriod.Sum(d => d.Resumen.TotalGravada + d.Resumen.TotalExenta);
        decimal totalDteTaxes = validDtesInPeriod.Where(d => d.TipoDte.Codigo == "03").Sum(d => d.Resumen.TotalIva);

        decimal totalManualSales = manualSales.Sum(s => s.TaxableAmount + s.ExemptAmount);
        decimal totalManualTaxes = manualSales.Sum(s => s.TaxAmount);

        decimal totalSales = totalDteSales + totalManualSales;
        decimal totalSalesTaxes = totalDteTaxes + totalManualTaxes;

        // Pending DTEs (Borrador, ErrorFinal, Rechazado) IN CURRENT PERIOD
        int pendingDtes = dtesInPeriod.Count(d => d.Estado == EstadoDte.Borrador || 
                                          d.Estado == EstadoDte.Rechazado || 
                                          d.Estado == EstadoDte.ErrorFinal);

        // Calculate Purchase Metrics IN CURRENT PERIOD
        var purchasesInPeriod = purchases.Where(p => p.IssueDate.Year == request.Year && p.IssueDate.Month == request.Month).ToList();
        decimal totalPurchases = purchasesInPeriod.Sum(p => p.SubTotal);
        decimal totalPurchaseTaxes = purchasesInPeriod.Sum(p => p.TaxAmount);

        // 📊 GRAPHICS DATA (LAST 6 MONTHS)
        var last6Months = Enumerable.Range(0, 6)
            .Select(i => new System.DateTime(request.Year, request.Month, 1).AddMonths(-i))
            .Reverse()
            .ToList();

        var monthlyPurchasesList = new System.Collections.Generic.List<MonthlyMetricDto>();
        decimal maxPurchaseAmount = 0;
        
        foreach (var m in last6Months)
        {
            var sum = purchases.Where(p => p.IssueDate.Year == m.Year && p.IssueDate.Month == m.Month).Sum(p => p.TotalAmount);
            if (sum > maxPurchaseAmount) maxPurchaseAmount = sum;
        }

        foreach (var m in last6Months)
        {
            var sum = purchases.Where(p => p.IssueDate.Year == m.Year && p.IssueDate.Month == m.Month).Sum(p => p.TotalAmount);
            double pct = maxPurchaseAmount == 0 ? 0 : (double)(sum / maxPurchaseAmount);
            monthlyPurchasesList.Add(new MonthlyMetricDto(m.ToString("MMM", new System.Globalization.CultureInfo("es-ES")).ToUpperInvariant(), sum, pct));
        }

        // Simular composición de impuestos
        var taxComp = new TaxCompositionDto(
            totalPurchaseTaxes, 
            totalPurchaseTaxes * 0.15m, 
            totalPurchaseTaxes * 0.05m, 
            totalPurchaseTaxes * 0.02m);

        return new DashboardMetricsDto
        {
            TotalSales = totalSales,
            TotalSalesTaxes = totalSalesTaxes,
            TotalPurchases = totalPurchases,
            TotalPurchaseTaxes = totalPurchaseTaxes,
            PendingDtesCount = pendingDtes,
            MonthlyPurchases = monthlyPurchasesList,
            TaxComposition = taxComp
        };
    }
}
