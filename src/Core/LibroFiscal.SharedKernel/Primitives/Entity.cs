namespace LibroFiscal.SharedKernel.Primitives;

/// <summary>
/// Base class for all domain entities.
/// Provides identity equality and domain event support through <see cref="AggregateRoot"/>.
/// </summary>
/// <typeparam name="TId">The type of the entity identifier.</typeparam>
public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull
{
    public TId Id { get; protected init; } = default!;

    protected Entity() { }

    protected Entity(TId id)
    {
        Id = id;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return obj is Entity<TId> entity && EqualityComparer<TId>.Default.Equals(Id, entity.Id);
    }

    public bool Equals(Entity<TId>? other) =>
        other is not null && EqualityComparer<TId>.Default.Equals(Id, other.Id);

    public override int GetHashCode() => EqualityComparer<TId>.Default.GetHashCode(Id);

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right) =>
        Equals(left, right);

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right) =>
        !Equals(left, right);
}
