using LibroFiscal.SharedKernel.Results;
using MediatR;

namespace LibroFiscal.Application.Abstractions.Messaging;

/// <summary>
/// Handler for queries.
/// </summary>
/// <typeparam name="TQuery">The query type.</typeparam>
/// <typeparam name="TResponse">The type of data returned.</typeparam>
public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}
