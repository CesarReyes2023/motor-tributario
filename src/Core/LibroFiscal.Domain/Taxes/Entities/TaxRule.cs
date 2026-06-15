using LibroFiscal.Domain.Taxes.Enums;
using LibroFiscal.SharedKernel.Primitives;

namespace LibroFiscal.Domain.Taxes.Entities;

public sealed class TaxRule : AuditableAggregateRoot<Guid>
{
    public string Name { get; private set; }
    public string Code { get; private set; }
    public decimal Rate { get; private set; }
    public TaxType Type { get; private set; }
    public bool IsActive { get; private set; }

    private TaxRule() 
    { 
        Name = null!;
        Code = null!;
        Type = null!;
    } // EF Core

    private TaxRule(Guid id, string name, string code, decimal rate, TaxType type, bool isActive) : base(id)
    {
        Name = name;
        Code = code;
        Rate = rate;
        Type = type;
        IsActive = isActive;
    }

    public static TaxRule Create(string name, string code, decimal rate, TaxType type)
    {
        return new TaxRule(Guid.NewGuid(), name, code, rate, type, true);
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }
}
