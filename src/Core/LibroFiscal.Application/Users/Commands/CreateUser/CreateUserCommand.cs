using LibroFiscal.Domain.Users.Enums;
using LibroFiscal.Domain.Users.Ids;
using LibroFiscal.SharedKernel.Results;
using MediatR;
using LibroFiscal.Domain.Users.Entities;
using LibroFiscal.SharedKernel.Interfaces;
using LibroFiscal.Application.Abstractions.Services;

namespace LibroFiscal.Application.Users.Commands.CreateUser;

public sealed record CreateUserCommand(
    string Username,
    string Password,
    UserRole Role) : IRequest<Result<UserId>>;

internal sealed class CreateUserCommandHandler(
    IRepository<User, UserId> userRepository,
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher) 
    : IRequestHandler<CreateUserCommand, Result<UserId>>
{
    public async Task<Result<UserId>> Handle(
        CreateUserCommand request, 
        CancellationToken cancellationToken)
    {
        var existingUsers = await userRepository.FindAsync(u => u.Username == request.Username, cancellationToken);
        if (existingUsers.Count > 0)
        {
            return Result.Failure<UserId>(Error.Conflict("User.Duplicate", "Username is already taken."));
        }

        var passwordHash = passwordHasher.Hash(request.Password);
        
        var user = User.Create(request.Username, passwordHash, request.Role);

        userRepository.Add(user);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}
