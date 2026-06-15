using LibroFiscal.SharedKernel.Results;
using MediatR;

using LibroFiscal.Application.Abstractions.Messaging;

namespace LibroFiscal.Application.Users.Queries.AuthenticateUser;

public sealed record AuthenticateUserQuery(string Username, string Password) : IQuery<AuthenticationResultDto>;

public sealed record AuthenticationResultDto(Guid UserId, string Username, string RoleName);
