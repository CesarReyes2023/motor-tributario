using LibroFiscal.Domain.Taxes.Entities;
using LibroFiscal.Domain.Taxes.Enums;
using LibroFiscal.Domain.Taxes.ValueObjects;

namespace LibroFiscal.Domain.Taxes.Services;

public sealed class TaxEngine : ITaxEngine
{
    public TaxCalculationResult Calculate(decimal subtotal, IEnumerable<TaxRule> applicableRules)
    {
        var taxDetails = new Dictionary<string, decimal>();
        decimal totalTaxes = 0;
        decimal totalDeductions = 0;

        foreach (var rule in applicableRules)
        {
            if (!rule.IsActive)
                continue;

            // Simple percentage calculation. For El Salvador, IVA is 13% of subtotal.
            // Retención is 1% of subtotal.
            // Monto = subtotal * (Rate / 100)
            decimal amount = Math.Round(subtotal * (rule.Rate / 100m), 2, MidpointRounding.AwayFromZero);

            taxDetails[rule.Code] = amount;

            if (rule.Type == TaxType.Addition)
            {
                totalTaxes += amount;
            }
            else if (rule.Type == TaxType.Deduction)
            {
                totalDeductions += amount;
            }
        }

        decimal grandTotal = subtotal + totalTaxes - totalDeductions;

        return new TaxCalculationResult(subtotal, totalTaxes, totalDeductions, grandTotal, taxDetails);
    }
}
