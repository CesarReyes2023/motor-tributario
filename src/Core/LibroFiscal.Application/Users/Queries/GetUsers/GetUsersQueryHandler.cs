using LibroFiscal.SharedKernel.Results;
using MediatR;
using LibroFiscal.SharedKernel.Interfaces;
using LibroFiscal.Domain.Users.Entities;
using LibroFiscal.Domain.Users.Ids;

namespace LibroFiscal.Application.Users.Queries.GetUsers;

internal sealed class GetUsersQueryHandler(IRepository<User, UserId> userRepository) 
    : IRequestHandler<GetUsersQuery, Result<IReadOnlyCollection<UserDto>>>
{
    public async Task<Result<IReadOnlyCollection<UserDto>>> Handle(
        GetUsersQuery request, 
        CancellationToken cancellationToken)
    {
        var users = await userRepository.FindAsync(u => true, cancellationToken);

        var dtos = users.Select(u => new UserDto(
            u.Id,
            u.Username,
            u.Role.Name,
            u.IsActive,
            u.CompanyAccesses.Count
        )).ToList();

        return dtos.AsReadOnly();
    }
}
