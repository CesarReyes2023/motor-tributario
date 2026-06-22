using LibroFiscal.SharedKernel.Primitives;

namespace LibroFiscal.Domain.Common.Ids;

/// <summary>Strongly-typed identifier for Company aggregate.</summary>
public sealed record CompanyId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static CompanyId New() => new(Guid.NewGuid());
    public static CompanyId From(Guid value) => new(value);
}

/// <summary>Strongly-typed identifier for DTE Document aggregate.</summary>
public sealed record DteId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static DteId New() => new(Guid.NewGuid());
    public static DteId From(Guid value) => new(value);
}

/// <summary>Strongly-typed identifier for Fiscal Book (Libro IVA) aggregate.</summary>
public sealed record LibroIvaId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static LibroIvaId New() => new(Guid.NewGuid());
    public static LibroIvaId From(Guid value) => new(value);
}

/// <summary>Strongly-typed identifier for Fiscal Rule aggregate.</summary>
public sealed record FiscalRuleId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static FiscalRuleId New() => new(Guid.NewGuid());
    public static FiscalRuleId From(Guid value) => new(value);
}

/// <summary>Strongly-typed identifier for Fiscal Catalog aggregate.</summary>
public sealed record CatalogoFiscalId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static CatalogoFiscalId New() => new(Guid.NewGuid());
    public static CatalogoFiscalId From(Guid value) => new(value);
}

/// <summary>Strongly-typed identifier for Audit Entry.</summary>
public sealed record AuditEntryId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static AuditEntryId New() => new(Guid.NewGuid());
    public static AuditEntryId From(Guid value) => new(value);
}

/// <summary>Strongly-typed identifier for Establishment (Sucursal/Punto de venta).</summary>
public sealed record EstablishmentId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static EstablishmentId New() => new(Guid.NewGuid());
    public static EstablishmentId From(Guid value) => new(value);
}

/// <summary>Strongly-typed identifier for Account (Cuenta Contable).</summary>
public sealed record AccountId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static AccountId New() => new(Guid.NewGuid());
    public static AccountId From(Guid value) => new(value);
}

/// <summary>Strongly-typed identifier for Journal Entry (Póliza Contable).</summary>
public sealed record JournalEntryId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static JournalEntryId New() => new(Guid.NewGuid());
    public static JournalEntryId From(Guid value) => new(value);
}

/// <summary>Strongly-typed identifier for Purchase (Compra).</summary>
public sealed record PurchaseId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static PurchaseId New() => new(Guid.NewGuid());
    public static PurchaseId From(Guid value) => new(value);
}

/// <summary>Strongly-typed identifier for Sale (Venta).</summary>
public sealed record SaleId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static SaleId New() => new(Guid.NewGuid());
    public static SaleId From(Guid value) => new(value);
}

/// <summary>Strongly-typed identifier for Invoice HTML Template.</summary>
public sealed record InvoiceTemplateId(Guid Value) : StronglyTypedId<Guid>(Value)
{
    public static InvoiceTemplateId New() => new(Guid.NewGuid());
    public static InvoiceTemplateId From(Guid value) => new(value);
}
