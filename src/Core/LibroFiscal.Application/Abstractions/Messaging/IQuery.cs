using LibroFiscal.SharedKernel.Results;
using MediatR;

namespace LibroFiscal.Application.Abstractions.Messaging;

/// <summary>
/// Marker interface for queries (read operations).
/// Queries never mutate state — they only return data.
/// Processed by a single handler.
/// </summary>
/// <typeparam name="TResponse">The type of data returned by the query.</typeparam>
public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}
