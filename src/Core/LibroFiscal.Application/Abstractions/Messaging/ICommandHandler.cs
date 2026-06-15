using LibroFiscal.SharedKernel.Results;
using MediatR;

namespace LibroFiscal.Application.Abstractions.Messaging;

/// <summary>
/// Handler for commands without a return value.
/// </summary>
public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : ICommand
{
}

/// <summary>
/// Handler for commands that return a typed value on success.
/// </summary>
/// <typeparam name="TCommand">The command type.</typeparam>
/// <typeparam name="TResponse">The type returned on success.</typeparam>
public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>
{
}
