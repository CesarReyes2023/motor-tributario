using FluentValidation;
using LibroFiscal.SharedKernel.Results;
using MediatR;

namespace LibroFiscal.Application.Abstractions.Behaviors;

/// <summary>
/// MediatR pipeline behavior that validates commands/queries using FluentValidation
/// before they reach their handlers. If validation fails, returns a ValidationResult
/// with all errors — the handler is never invoked.
/// 
/// This centralizes validation logic and ensures no invalid command/query
/// ever reaches the domain layer.
/// </summary>
/// <typeparam name="TRequest">The command or query type.</typeparam>
/// <typeparam name="TResponse">The response type (must be a Result).</typeparam>
public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var errors = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .Select(f => Error.Validation(f.PropertyName, f.ErrorMessage))
            .Distinct()
            .ToArray();

        if (errors.Length != 0)
        {
            return CreateValidationResult<TResponse>(errors);
        }

        return await next();
    }

    private static TResponse CreateValidationResult<T>(Error[] errors)
        where T : Result
    {
        if (typeof(T) == typeof(Result))
        {
            return (TResponse)(object)ValidationResult.WithErrors(errors);
        }

        // For Result<TValue>, we need to create ValidationResult<TValue>
        var resultType = typeof(T).GetGenericArguments()[0];
        var validationResultType = typeof(ValidationResult<>).MakeGenericType(resultType);
        var withErrorsMethod = validationResultType.GetMethod(nameof(ValidationResult.WithErrors))!;

        return (TResponse)withErrorsMethod.Invoke(null, [errors])!;
    }
}
