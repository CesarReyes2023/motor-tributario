using System.Diagnostics.CodeAnalysis;

namespace LibroFiscal.SharedKernel.Results;

/// <summary>
/// Result monad for operations that can succeed or fail.
/// Replaces exceptions for expected business failures.
/// 
/// Forces callers to explicitly handle both success and failure paths.
/// All fiscal operations (DTE validation, tax calculation, signing) return Result&lt;T&gt;.
/// 
/// Usage:
/// <code>
/// Result&lt;DteDocument&gt; result = fiscalEngine.Validate(dte);
/// if (result.IsFailure)
///     return result.Error; // propagate error
/// var validDte = result.Value; // safe to use
/// </code>
/// </summary>
public class Result
{
    public bool IsSuccess { get; }

    [MemberNotNullWhen(false, nameof(IsSuccess))]
    public bool IsFailure => !IsSuccess;

    public Error Error { get; }

    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
            throw new InvalidOperationException("Cannot create a successful result with an error.");
        if (!isSuccess && error == Error.None)
            throw new InvalidOperationException("Cannot create a failed result without an error.");

        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, Error.None);

    public static Result Failure(Error error) => new(false, error);

    /// <summary>
    /// Implicit conversion from Error to failed Result.
    /// Enables: return Error.Validation("code", "msg"); in methods returning Result.
    /// </summary>
    public static implicit operator Result(Error error) => Failure(error);

    public static Result<TValue> Success<TValue>(TValue value) =>
        new(value, true, Error.None);

    public static Result<TValue> Failure<TValue>(Error error) =>
        new(default, false, error);

    /// <summary>
    /// Creates a Result from a value that might be null.
    /// Returns failure with the provided error if the value is null.
    /// </summary>
    public static Result<TValue> Create<TValue>(TValue? value, Error error) =>
        value is not null ? Success(value) : Failure<TValue>(error);
}

/// <summary>
/// Generic result carrying a success value of type <typeparamref name="TValue"/>.
/// </summary>
/// <typeparam name="TValue">The type of the success value.</typeparam>
public class Result<TValue> : Result
{
    private readonly TValue? _value;

    /// <summary>
    /// The success value. Throws if accessed on a failed result.
    /// </summary>
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException($"Cannot access Value on a failed Result. Error: {Error}");

    internal Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    /// <summary>
    /// Implicit conversion from TValue to Result&lt;TValue&gt;.
    /// Allows returning values directly from methods that return Result&lt;T&gt;.
    /// </summary>
    public static implicit operator Result<TValue>(TValue? value) =>
        value is not null ? Success(value) : Failure<TValue>(Error.NullValue);

    /// <summary>
    /// Implicit conversion from Error to Result&lt;TValue&gt;.
    /// Allows returning errors directly from methods that return Result&lt;T&gt;.
    /// </summary>
    public static implicit operator Result<TValue>(Error error) =>
        Failure<TValue>(error);

    /// <summary>
    /// Maps the success value using the provided function.
    /// If this result is a failure, the error is propagated without executing the function.
    /// </summary>
    public Result<TNext> Map<TNext>(Func<TValue, TNext> mapper)
    {
        return IsSuccess
            ? Result.Success(mapper(Value))
            : Result.Failure<TNext>(Error);
    }

    /// <summary>
    /// Flat-maps (bind) the success value using the provided function that returns a Result.
    /// Enables chaining of Result-returning operations.
    /// </summary>
    public Result<TNext> Bind<TNext>(Func<TValue, Result<TNext>> binder)
    {
        return IsSuccess ? binder(Value) : Result.Failure<TNext>(Error);
    }

    /// <summary>
    /// Matches on success or failure, executing the appropriate function.
    /// Forces exhaustive handling of both paths.
    /// </summary>
    public TResult Match<TResult>(
        Func<TValue, TResult> onSuccess,
        Func<Error, TResult> onFailure)
    {
        return IsSuccess ? onSuccess(Value) : onFailure(Error);
    }
}
