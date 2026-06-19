using LibroFiscal.Application.Abstractions.Messaging;
using LibroFiscal.SharedKernel.Results;
using MediatR;

namespace LibroFiscal.Application.Users.Commands.UpdateProfilePicture;

public sealed record UpdateProfilePictureCommand(
    Guid UserId,
    string ProfilePicturePath) : ICommand;
