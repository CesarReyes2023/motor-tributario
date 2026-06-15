using LibroFiscal.Application.Abstractions.Messaging;
using LibroFiscal.Application.Abstractions.Services;
using LibroFiscal.Domain.Users.Entities;
using LibroFiscal.Domain.Users.Ids;
using LibroFiscal.SharedKernel.Interfaces;
using LibroFiscal.SharedKernel.Results;

namespace LibroFiscal.Application.Users.Queries.AuthenticateUser;

public sealed class AuthenticateUserQueryHandler : IQueryHandler<AuthenticateUserQuery, AuthenticationResultDto>
{
    private readonly IRepository<User, UserId> _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public AuthenticateUserQueryHandler(IRepository<User, UserId> userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<AuthenticationResultDto>> Handle(AuthenticateUserQuery request, CancellationToken cancellationToken)
    {
        var users = await _userRepository.FindAsync(u => u.Username == request.Username, cancellationToken);
        var user = users.Count > 0 ? users[0] : null;

        if (user is null || !user.IsActive)
        {
            return Result.Failure<AuthenticationResultDto>(Error.Unauthorized("Auth.Failed", "Usuario o contraseña incorrectos."));
        }

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return Result.Failure<AuthenticationResultDto>(Error.Unauthorized("Auth.Failed", "Usuario o contraseña incorrectos."));
        }

        return new AuthenticationResultDto(user.Id.Value, user.Username, user.Role.Name);
    }
}
