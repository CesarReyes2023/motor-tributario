using LibroFiscal.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.Logging;

#pragma warning disable CA1848 // Use LoggerMessage delegates — suppressed in pipeline behaviors

namespace LibroFiscal.Application.Abstractions.Behaviors;

/// <summary>
/// MediatR pipeline behavior that wraps command execution in a transaction scope.
/// Ensures atomicity: all changes within a command handler either commit together
/// or roll back together.
/// 
/// Only applies to commands (write operations), not queries.
/// The transaction behavior calls SaveChangesAsync on the UnitOfWork after
/// the handler succeeds, and dispatches domain events post-commit.
/// </summary>
/// <typeparam name="TRequest">The command type.</typeparam>
/// <typeparam name="TResponse">The response type (must be a Result).</typeparam>
public sealed class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    private readonly SharedKernel.Interfaces.IUnitOfWork _unitOfWork;
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;

    public TransactionBehavior(
        SharedKernel.Interfaces.IUnitOfWork unitOfWork,
        ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Only apply to commands (write operations)
        if (!IsCommand())
            return await next();

        string requestName = typeof(TRequest).Name;

        _logger.LogDebug("[TXN] Starting transaction for {RequestName}", requestName);

        var response = await next();

        if (response.IsSuccess)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogDebug("[TXN] Committed transaction for {RequestName}", requestName);
        }
        else
        {
            _logger.LogDebug("[TXN] Skipping commit for failed {RequestName}: {Error}",
                requestName, response.Error);
        }

        return response;
    }

    private static bool IsCommand()
    {
        return typeof(TRequest).GetInterfaces().Any(i =>
            i == typeof(Messaging.ICommand) ||
            (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(Messaging.ICommand<>)));
    }
}
