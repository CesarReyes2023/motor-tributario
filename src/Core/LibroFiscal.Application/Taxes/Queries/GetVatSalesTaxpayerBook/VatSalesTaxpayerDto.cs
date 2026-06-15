namespace LibroFiscal.Application.Taxes.Queries.GetVatSalesTaxpayerBook;

public sealed record VatSalesTaxpayerDto
{
    public DateTimeOffset EmisionDate { get; init; }
    public string DocumentNumber { get; init; } = string.Empty;
    public string NrcCustomer { get; init; } = string.Empty;
    public string CustomerName { get; init; } = string.Empty;
    
    public decimal ExemptSales { get; init; }
    public decimal LocalGravadaSales { get; init; }
    public decimal FiscalDebit { get; init; }
    public decimal RetainedIva { get; init; }
    public decimal TotalSales { get; init; }
}
