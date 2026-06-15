#pragma warning disable CA1716 // Type name 'Error' conflicts with reserved keyword — intentional: standard Result pattern naming

namespace LibroFiscal.SharedKernel.Results;

/// <summary>
/// Represents a structured, domain-meaningful error.
/// Errors have a code (machine-readable) and message (human-readable).
/// 
/// Error types align with fiscal/business error categories:
/// - Validation: input/format errors
/// - NotFound: entity doesn't exist
/// - Conflict: duplicate or state conflict
/// - Failure: general business rule violation
/// - Unauthorized: permission denied
/// - External: third-party system error (e.g., Hacienda API)
/// </summary>
public sealed record Error
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.None);
    public static readonly Error NullValue = new("General.NullValue", "A null value was provided.", ErrorType.Validation);

    public string Code { get; }
    public string Message { get; }
    public ErrorType Type { get; }

    private Error(string code, string message, ErrorType type)
    {
        Code = code;
        Message = message;
        Type = type;
    }

    public static Error Validation(string code, string message) =>
        new(code, message, ErrorType.Validation);

    public static Error NotFound(string code, string message) =>
        new(code, message, ErrorType.NotFound);

    public static Error Conflict(string code, string message) =>
        new(code, message, ErrorType.Conflict);

    public static Error Failure(string code, string message) =>
        new(code, message, ErrorType.Failure);

    public static Error Unauthorized(string code, string message) =>
        new(code, message, ErrorType.Unauthorized);

    public static Error External(string code, string message) =>
        new(code, message, ErrorType.External);

    public override string ToString() => $"[{Type}] {Code}: {Message}";
}

/// <summary>
/// Categorizes errors for proper handling at each layer.
/// </summary>
public enum ErrorType
{
    None = 0,
    Validation = 1,
    NotFound = 2,
    Conflict = 3,
    Failure = 4,
    Unauthorized = 5,
    External = 6
}
