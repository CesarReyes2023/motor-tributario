using LibroFiscal.Domain.Taxes.Entities;
using LibroFiscal.Domain.Taxes.ValueObjects;

namespace LibroFiscal.Domain.Taxes.Services;

public interface ITaxEngine
{
    TaxCalculationResult Calculate(decimal subtotal, IEnumerable<TaxRule> applicableRules);
}
