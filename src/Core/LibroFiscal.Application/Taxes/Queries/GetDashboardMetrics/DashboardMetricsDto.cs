namespace LibroFiscal.Application.Taxes.Queries.GetDashboardMetrics;

public sealed record DashboardMetricsDto
{
    public decimal TotalSales { get; init; }
    public decimal TotalSalesTaxes { get; init; }
    
    public decimal TotalPurchases { get; init; }
    public decimal TotalPurchaseTaxes { get; init; }

    public decimal IvaBalance => TotalSalesTaxes - TotalPurchaseTaxes;

    public int PendingDtesCount { get; init; }
    
    // For chart proportionality (0.0 to 1.0)
    public double SalesPercentage => (TotalSales + TotalPurchases) == 0 ? 0.5 : (double)(TotalSales / (TotalSales + TotalPurchases));
    public double PurchasesPercentage => (TotalSales + TotalPurchases) == 0 ? 0.5 : (double)(TotalPurchases / (TotalSales + TotalPurchases));
}
