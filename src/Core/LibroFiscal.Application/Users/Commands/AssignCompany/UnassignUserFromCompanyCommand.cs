using LibroFiscal.Domain.Common.Ids;
using LibroFiscal.Domain.Users.Ids;
using LibroFiscal.Domain.Users.Entities;
using LibroFiscal.Domain.Companies.Entities;
using LibroFiscal.SharedKernel.Results;
using MediatR;
using LibroFiscal.SharedKernel.Interfaces;

namespace LibroFiscal.Application.Users.Commands.AssignCompany;

public sealed record UnassignUserFromCompanyCommand(
    UserId UserId,
    CompanyId CompanyId) : IRequest<Result>;

internal sealed class UnassignUserFromCompanyCommandHandler(
    IRepository<User, UserId> userRepository,
    IRepository<Company, CompanyId> companyRepository,
    IUnitOfWork unitOfWork) 
    : IRequestHandler<UnassignUserFromCompanyCommand, Result>
{
    public async Task<Result> Handle(
        UnassignUserFromCompanyCommand request, 
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

        user.RemoveCompany(request.CompanyId);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
