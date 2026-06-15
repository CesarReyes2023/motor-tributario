using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

#pragma warning disable CA1848 // Use LoggerMessage delegates — suppressed in pipeline behaviors where templates are dynamic

namespace LibroFiscal.Application.Abstractions.Behaviors;

/// <summary>
/// MediatR pipeline behavior that logs command/query execution with timing.
/// Logs warnings for operations exceeding 500ms threshold.
/// Includes request name and structured properties for observability.
/// </summary>
/// <typeparam name="TRequest">The command or query type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    private const int SlowThresholdMs = 500;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        string requestName = typeof(TRequest).Name;

        _logger.LogInformation(
            "[START] {RequestName}",
            requestName);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next();
            stopwatch.Stop();

            if (stopwatch.ElapsedMilliseconds > SlowThresholdMs)
            {
                _logger.LogWarning(
                    "[SLOW] {RequestName} took {ElapsedMs}ms (threshold: {ThresholdMs}ms)",
                    requestName,
                    stopwatch.ElapsedMilliseconds,
                    SlowThresholdMs);
            }
            else
            {
                _logger.LogInformation(
                    "[END] {RequestName} completed in {ElapsedMs}ms",
                    requestName,
                    stopwatch.ElapsedMilliseconds);
            }

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(
                ex,
                "[ERROR] {RequestName} failed after {ElapsedMs}ms",
                requestName,
                stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
