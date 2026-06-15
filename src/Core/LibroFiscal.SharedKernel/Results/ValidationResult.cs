#pragma warning disable CA1000 // Static members on generic types — intentional: factory methods on Result<T> are a standard pattern

namespace LibroFiscal.SharedKernel.Results;

/// <summary>
/// Result type that can carry multiple validation errors.
/// Used when an operation can fail due to several field-level issues simultaneously
/// (e.g., DTE validation where emisor, receptor, and cuerpo all have errors).
/// </summary>
public sealed class ValidationResult : Result
{
    public Error[] Errors { get; }

    private ValidationResult(Error[] errors)
        : base(false, errors.Length > 0 ? errors[0] : Error.NullValue)
    {
        Errors = errors;
    }

    public static ValidationResult WithErrors(params Error[] errors)
    {
        if (errors.Length == 0)
            throw new ArgumentException("At least one error is required for a validation failure.", nameof(errors));

        return new ValidationResult(errors);
    }

    public static ValidationResult Ok() => new([]);
}

/// <summary>
/// Generic validation result carrying a success value or multiple validation errors.
/// </summary>
/// <typeparam name="TValue">The type of the success value.</typeparam>
public sealed class ValidationResult<TValue> : Result<TValue>
{
    public Error[] Errors { get; }

    private ValidationResult(TValue? value, Error[] errors)
        : base(value, errors.Length == 0, errors.Length > 0 ? errors[0] : Error.None)
    {
        Errors = errors;
    }

    public static ValidationResult<TValue> WithErrors(params Error[] errors)
    {
        if (errors.Length == 0)
            throw new ArgumentException("At least one error is required for a validation failure.", nameof(errors));

        return new ValidationResult<TValue>(default, errors);
    }

    public static ValidationResult<TValue> Ok(TValue value) => new(value, []);
}
