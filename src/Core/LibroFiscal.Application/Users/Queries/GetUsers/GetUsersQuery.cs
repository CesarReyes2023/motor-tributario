using LibroFiscal.Domain.Users.Enums;
using LibroFiscal.Domain.Users.Ids;
using LibroFiscal.SharedKernel.Results;
using MediatR;

namespace LibroFiscal.Application.Users.Queries.GetUsers;

public sealed record UserDto(
    UserId Id,
    string Username,
    string Role,
    bool IsActive,
    int AssignedCompaniesCount);

public sealed record GetUsersQuery() : IRequest<Result<IReadOnlyCollection<UserDto>>>;
