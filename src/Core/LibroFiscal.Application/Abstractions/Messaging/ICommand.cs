using LibroFiscal.SharedKernel.Results;
using MediatR;

namespace LibroFiscal.Application.Abstractions.Messaging;

/// <summary>
/// Marker interface for commands (write operations).
/// Commands mutate state and return Result for explicit error handling.
/// Processed by a single handler.
/// </summary>
public interface ICommand : IRequest<Result>
{
}

/// <summary>
/// Command that returns a typed result value on success.
/// </summary>
/// <typeparam name="TResponse">The type returned on successful execution.</typeparam>
public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}
