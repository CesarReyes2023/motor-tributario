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

    // Phase 2: Graphs data
    public System.Collections.Generic.List<MonthlyMetricDto> MonthlyPurchases { get; init; } = new();
    public TaxCompositionDto TaxComposition { get; init; } = new(0, 0, 0, 0);
}

public sealed record MonthlyMetricDto(string MonthLabel, decimal Amount, double PercentageMax);

public sealed record TaxCompositionDto(decimal Iva, decimal Renta, decimal Fovial, decimal Cotrans)
{
    public decimal TotalTaxes => Iva + Renta + Fovial + Cotrans;
    public double IvaPercentage => TotalTaxes == 0 ? 0 : (double)(Iva / TotalTaxes);
    public double RentaPercentage => TotalTaxes == 0 ? 0 : (double)(Renta / TotalTaxes);
    public double FovialPercentage => TotalTaxes == 0 ? 0 : (double)(Fovial / TotalTaxes);
    public double CotransPercentage => TotalTaxes == 0 ? 0 : (double)(Cotrans / TotalTaxes);
}
