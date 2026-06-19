using LibroFiscal.Application.Abstractions.Messaging;
using LibroFiscal.Domain.Users.Entities;
using LibroFiscal.Domain.Users.Ids;
using LibroFiscal.SharedKernel.Interfaces;
using LibroFiscal.SharedKernel.Results;

namespace LibroFiscal.Application.Users.Commands.UpdateProfilePicture;

public sealed class UpdateProfilePictureCommandHandler : ICommandHandler<UpdateProfilePictureCommand>
{
    private readonly IRepository<User, UserId> _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProfilePictureCommandHandler(IRepository<User, UserId> userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateProfilePictureCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(new UserId(request.UserId), cancellationToken);

        if (user is null)
        {
            return Result.Failure(Error.NotFound("User.NotFound", "El usuario no existe."));
        }

        user.UpdateProfilePicture(request.ProfilePicturePath);

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
