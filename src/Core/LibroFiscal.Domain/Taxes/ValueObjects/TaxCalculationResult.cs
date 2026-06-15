using LibroFiscal.Domain.Taxes.Entities;
using LibroFiscal.SharedKernel.Primitives;

namespace LibroFiscal.Domain.Taxes.ValueObjects;

public sealed class TaxCalculationResult : ValueObject
{
    public decimal Subtotal { get; }
    public decimal TotalTaxes { get; }
    public decimal TotalDeductions { get; }
    public decimal GrandTotal { get; }

    /// <summary>
    /// Desglose de impuestos individuales aplicados.
    /// Key: Tax Code (ej: IVA, RET)
    /// Value: Amount calculado
    /// </summary>
    public IReadOnlyDictionary<string, decimal> TaxDetails { get; }

    public TaxCalculationResult(
        decimal subtotal, 
        decimal totalTaxes, 
        decimal totalDeductions, 
        decimal grandTotal, 
        Dictionary<string, decimal> taxDetails)
    {
        Subtotal = subtotal;
        TotalTaxes = totalTaxes;
        TotalDeductions = totalDeductions;
        GrandTotal = grandTotal;
        TaxDetails = taxDetails;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Subtotal;
        yield return TotalTaxes;
        yield return TotalDeductions;
        yield return GrandTotal;
        
        foreach (var kvp in TaxDetails.OrderBy(k => k.Key))
        {
            yield return kvp.Key;
            yield return kvp.Value;
        }
    }
}
