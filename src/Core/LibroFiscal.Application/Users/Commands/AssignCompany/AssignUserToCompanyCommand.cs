using LibroFiscal.Domain.Common.Ids;
using LibroFiscal.Domain.Users.Ids;
using LibroFiscal.Domain.Users.Entities;
using LibroFiscal.Domain.Companies.Entities;
using LibroFiscal.SharedKernel.Results;
using MediatR;
using LibroFiscal.SharedKernel.Interfaces;

namespace LibroFiscal.Application.Users.Commands.AssignCompany;

public sealed record AssignUserToCompanyCommand(UserId UserId, CompanyId CompanyId) : IRequest<Result>;

internal sealed class AssignUserToCompanyCommandHandler(
    IRepository<User, UserId> userRepository,
    IRepository<Company, CompanyId> companyRepository,
    IUnitOfWork unitOfWork) 
    : IRequestHandler<AssignUserToCompanyCommand, Result>
{
    public async Task<Result> Handle(
        AssignUserToCompanyCommand request, 
        CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure(Error.NotFound("User.NotFound", "User not found."));
        }

        var company = await companyRepository.GetByIdAsync(request.CompanyId, cancellationToken);
        if (company is null)
        {
            return Result.Failure(Error.NotFound("Company.NotFound", "Company not found."));
        }

        user.AssignCompany(request.CompanyId);
        
        userRepository.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
