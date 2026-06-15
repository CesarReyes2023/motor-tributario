namespace LibroFiscal.Application.Taxes.Queries.GetVatSalesConsumerBook;

public sealed record VatSalesConsumerDto
{
    public DateTimeOffset EmisionDate { get; init; }
    public string InitialDocumentNumber { get; init; } = string.Empty;
    public string FinalDocumentNumber { get; init; } = string.Empty;
    
    public decimal ExemptSales { get; init; }
    public decimal LocalGravadaSales { get; init; }
    public decimal ExportSales { get; init; }
    public decimal TotalSales { get; init; }
    public decimal RetainedIva { get; init; }
}
