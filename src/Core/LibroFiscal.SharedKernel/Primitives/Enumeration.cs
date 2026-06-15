using System.Reflection;

namespace LibroFiscal.SharedKernel.Primitives;

/// <summary>
/// Smart enum pattern. Replaces magic strings/ints with type-safe, behavior-rich enumerations.
/// Each enumeration value has a numeric Id and a display Name.
/// 
/// Usage:
/// <code>
/// public class TipoDte : Enumeration
/// {
///     public static readonly TipoDte Factura = new(1, "01", "Factura");
///     public static readonly TipoDte CreditoFiscal = new(3, "03", "Crédito Fiscal");
///     
///     public string Codigo { get; }
///     private TipoDte(int id, string codigo, string name) : base(id, name) => Codigo = codigo;
/// }
/// </code>
/// </summary>
public abstract class Enumeration : IComparable
{
    public int Id { get; }
    public string Name { get; }

    protected Enumeration(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public override string ToString() => Name;

    public override bool Equals(object? obj)
    {
        if (obj is not Enumeration otherValue) return false;
        bool typeMatches = GetType() == obj.GetType();
        bool valueMatches = Id.Equals(otherValue.Id);
        return typeMatches && valueMatches;
    }

    public override int GetHashCode() => Id.GetHashCode();

    public int CompareTo(object? obj) => Id.CompareTo(((Enumeration)obj!).Id);

    public static bool operator <(Enumeration left, Enumeration right) =>
        left.CompareTo(right) < 0;

    public static bool operator <=(Enumeration left, Enumeration right) =>
        left.CompareTo(right) <= 0;

    public static bool operator >(Enumeration left, Enumeration right) =>
        left.CompareTo(right) > 0;

    public static bool operator >=(Enumeration left, Enumeration right) =>
        left.CompareTo(right) >= 0;

    /// <summary>
    /// Returns all defined values for the enumeration type.
    /// Uses reflection to discover static fields — cached at first call per type.
    /// </summary>
    public static IEnumerable<T> GetAll<T>() where T : Enumeration
    {
        return typeof(T)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Select(f => f.GetValue(null))
            .OfType<T>();
    }

    /// <summary>
    /// Finds an enumeration value by its numeric ID.
    /// </summary>
    public static T FromId<T>(int id) where T : Enumeration
    {
        var matchingItem = GetAll<T>().FirstOrDefault(item => item.Id == id);
        return matchingItem ?? throw new InvalidOperationException(
            $"'{id}' is not a valid ID in {typeof(T)}. Valid values: {string.Join(", ", GetAll<T>().Select(e => $"{e.Id}={e.Name}"))}");
    }

    /// <summary>
    /// Finds an enumeration value by its display name.
    /// </summary>
    public static T FromName<T>(string name) where T : Enumeration
    {
        var matchingItem = GetAll<T>().FirstOrDefault(
            item => string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase));
        return matchingItem ?? throw new InvalidOperationException(
            $"'{name}' is not a valid name in {typeof(T)}. Valid values: {string.Join(", ", GetAll<T>().Select(e => e.Name))}");
    }

    /// <summary>
    /// Attempts to find an enumeration value by ID, returning null if not found.
    /// </summary>
    public static T? TryFromId<T>(int id) where T : Enumeration
    {
        return GetAll<T>().FirstOrDefault(item => item.Id == id);
    }

    public static bool operator ==(Enumeration? left, Enumeration? right) =>
        Equals(left, right);

    public static bool operator !=(Enumeration? left, Enumeration? right) =>
        !Equals(left, right);
}
