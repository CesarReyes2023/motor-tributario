using LibroFiscal.SharedKernel.Primitives;

namespace LibroFiscal.Domain.Taxes.Enums;

public sealed class TaxType : Enumeration
{
    public static readonly TaxType Addition = new(1, "Addition"); // Suma al subtotal (ej: IVA)
    public static readonly TaxType Deduction = new(2, "Deduction"); // Resta al subtotal (ej: Retención)

    private TaxType(int id, string name) : base(id, name)
    {
    }
}
