namespace LibroFiscal.SharedKernel.Primitives;

/// <summary>
/// Base class for value objects. Value objects are identified by their structural equality
/// (all properties must match), not by an identity field.
/// 
/// Value objects are immutable and should be created via constructors or factory methods.
/// Examples: Money, Address, FiscalPeriod, NIT, NumeroControl.
/// </summary>
public abstract class ValueObject : IEquatable<ValueObject>
{
    /// <summary>
    /// Returns the components used for equality comparison.
    /// All properties that define the value object's identity must be included.
    /// </summary>
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return obj is ValueObject valueObject && ValuesAreEqual(valueObject);
    }

    public bool Equals(ValueObject? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return GetType() == other.GetType() && ValuesAreEqual(other);
    }

    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Aggregate(
                seed: 0,
                func: (current, component) =>
                    HashCode.Combine(current, component?.GetHashCode() ?? 0));
    }

    public static bool operator ==(ValueObject? left, ValueObject? right) =>
        Equals(left, right);

    public static bool operator !=(ValueObject? left, ValueObject? right) =>
        !Equals(left, right);

    private bool ValuesAreEqual(ValueObject other)
    {
        return GetEqualityComponents()
            .SequenceEqual(other.GetEqualityComponents());
    }
}
