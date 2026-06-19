using LibroFiscal.Application.Abstractions.Messaging;
using LibroFiscal.SharedKernel.Results;
using MediatR;

namespace LibroFiscal.Application.Users.Commands.ChangePassword;

public sealed record ChangePasswordCommand(
    Guid UserId,
    string CurrentPassword,
    string NewPassword) : ICommand;
