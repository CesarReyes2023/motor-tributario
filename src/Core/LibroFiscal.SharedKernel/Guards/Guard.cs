using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

#pragma warning disable CA1822 // Mark members as static — intentional: fluent API pattern requires instance methods on GuardClause struct

namespace LibroFiscal.SharedKernel.Guards;

/// <summary>
/// Precondition guard clauses for defensive programming.
/// Guards validate method inputs and invariants, throwing <see cref="ArgumentException"/>
/// derivatives for programming errors (not business logic violations — use Result&lt;T&gt; for those).
/// 
/// Usage:
/// <code>
/// public DteDocument(DteId id, string codigoGeneracion, TipoDte tipo)
/// {
///     Guard.Against.NullOrEmpty(codigoGeneracion);
///     Guard.Against.Null(tipo);
///     Guard.Against.InvalidFormat(codigoGeneracion, UuidV4Pattern, "Código de generación");
/// }
/// </code>
/// </summary>
public static class Guard
{
    public static GuardClause Against => new();
}

public readonly struct GuardClause
{
    /// <summary>
    /// Throws if value is null.
    /// </summary>
    public T Null<T>(
        [NotNull] T? value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (value is null)
            throw new ArgumentNullException(paramName);
        return value;
    }

    /// <summary>
    /// Throws if string is null or empty.
    /// </summary>
    public string NullOrEmpty(
        [NotNull] string? value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (string.IsNullOrEmpty(value))
            throw new ArgumentException("Value cannot be null or empty.", paramName);
        return value;
    }

    /// <summary>
    /// Throws if string is null, empty, or whitespace.
    /// </summary>
    public string NullOrWhiteSpace(
        [NotNull] string? value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value cannot be null, empty, or whitespace.", paramName);
        return value;
    }

    /// <summary>
    /// Throws if Guid is empty (Guid.Empty).
    /// </summary>
    public Guid EmptyGuid(
        Guid value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Guid cannot be empty.", paramName);
        return value;
    }

    /// <summary>
    /// Throws if collection is null or empty.
    /// </summary>
    public IReadOnlyCollection<T> NullOrEmptyCollection<T>(
        [NotNull] IReadOnlyCollection<T>? value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (value is null || value.Count == 0)
            throw new ArgumentException("Collection cannot be null or empty.", paramName);
        return value;
    }

    /// <summary>
    /// Throws if numeric value is negative.
    /// </summary>
    public decimal Negative(
        decimal value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(paramName, value, "Value cannot be negative.");
        return value;
    }

    /// <summary>
    /// Throws if numeric value is zero or negative.
    /// </summary>
    public decimal ZeroOrNegative(
        decimal value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (value <= 0)
            throw new ArgumentOutOfRangeException(paramName, value, "Value must be positive.");
        return value;
    }

    /// <summary>
    /// Throws if value is outside the specified range [min, max].
    /// </summary>
    public T OutOfRange<T>(
        T value,
        T min,
        T max,
        [CallerArgumentExpression(nameof(value))] string? paramName = null) where T : IComparable<T>
    {
        if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
            throw new ArgumentOutOfRangeException(paramName, value, $"Value must be between {min} and {max}.");
        return value;
    }

    /// <summary>
    /// Throws if the condition is not met. Used for custom invariants.
    /// </summary>
    public void Condition(
        [DoesNotReturnIf(false)] bool condition,
        string message,
        [CallerArgumentExpression(nameof(condition))] string? paramName = null)
    {
        if (!condition)
            throw new ArgumentException(message, paramName);
    }
}
