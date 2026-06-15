namespace LibroFiscal.SharedKernel.Primitives;

/// <summary>
/// Base class for strongly-typed IDs. Prevents accidental ID mixing between aggregates.
/// 
/// A CompanyId cannot be passed where a DteId is expected, even though both wrap Guid.
/// This is critical in a multi-entity fiscal system where incorrect ID routing
/// could cause severe data integrity issues.
/// 
/// Usage:
/// <code>
/// public sealed record DteId(Guid Value) : StronglyTypedId&lt;Guid&gt;(Value)
/// {
///     public static DteId New() => new(Guid.NewGuid());
/// }
/// </code>
/// </summary>
/// <typeparam name="TValue">The underlying value type (typically Guid or long).</typeparam>
public abstract record StronglyTypedId<TValue>(TValue Value)
    where TValue : notnull
{
    public override string ToString() => Value.ToString()!;
}
