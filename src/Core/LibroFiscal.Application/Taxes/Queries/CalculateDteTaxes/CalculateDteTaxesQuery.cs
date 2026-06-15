using LibroFiscal.Application.Abstractions.Messaging;
using LibroFiscal.Domain.Taxes.ValueObjects;

namespace LibroFiscal.Application.Taxes.Queries.CalculateDteTaxes;

public sealed record CalculateDteTaxesQuery(decimal Subtotal) : IQuery<TaxCalculationResultDto>;

public sealed record TaxCalculationResultDto(
    decimal Subtotal,
    decimal TotalTaxes,
    decimal TotalDeductions,
    decimal GrandTotal,
    IReadOnlyDictionary<string, decimal> TaxDetails
);
