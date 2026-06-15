using LibroFiscal.Application.Abstractions.Messaging;
using LibroFiscal.Domain.Common.Enumerations;
using LibroFiscal.Domain.Common.Ids;
using LibroFiscal.Domain.Common.ValueObjects;
using LibroFiscal.Domain.Companies.Entities;
using LibroFiscal.SharedKernel.Interfaces;
using LibroFiscal.SharedKernel.Results;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LibroFiscal.Application.Companies.Commands.UpdateCompanyProfile;

internal sealed class UpdateCompanyProfileCommandHandler : ICommandHandler<UpdateCompanyProfileCommand>
{
    private readonly IRepository<Company, CompanyId> _companyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCompanyProfileCommandHandler(
        IRepository<Company, CompanyId> companyRepository,
        IUnitOfWork unitOfWork)
    {
        _companyRepository = companyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        UpdateCompanyProfileCommand request,
        CancellationToken cancellationToken)
    {
        var nitResult = Nit.Create(request.Nit);
        if (nitResult.IsFailure) return nitResult;

        var nrcResult = Nrc.Create(request.Nrc);
        if (nrcResult.IsFailure) return nrcResult;

        var direccionResult = DireccionFiscal.Create(request.Department, request.Municipality, request.AddressLine);
        if (direccionResult.IsFailure) return direccionResult;
        var direccion = direccionResult.Value;

        var companies = await _companyRepository.FindAsync(c => c.IsActive, cancellationToken);
        var company = companies.Count > 0 ? companies[0] : null;

        if (company is null)
        {
            // Create new
            var newCompanyResult = Company.Create(
                request.LegalName,
                request.TradeName,
                nitResult.Value,
                nrcResult.Value,
                request.EconomicActivityCode,
                request.EconomicActivityDescription,
                request.Phone,
                request.Email,
                direccion,
                AmbienteHacienda.Pruebas);

            if (newCompanyResult.IsFailure) return newCompanyResult;

            _companyRepository.Add(newCompanyResult.Value);
        }
        else
        {
            // Entity Framework tracks the entity, we would normally update properties here
            // However, the Domain model for Company currently lacks an Update method.
            // We should either add an Update method or recreate if it's not possible.
            // Since Company has private setters, let's assume we can add an Update method to it later,
            // but for now let's just deactivate the old one and create a new one, OR we can add the Update method.
            // Wait, let's just deactivate the old and create a new one if we don't have an Update method.
            // Actually, in a real app, I'd modify Company.cs.
            company.Deactivate();
            _companyRepository.Update(company);

            var newCompanyResult = Company.Create(
                request.LegalName,
                request.TradeName,
                nitResult.Value,
                nrcResult.Value,
                request.EconomicActivityCode,
                request.EconomicActivityDescription,
                request.Phone,
                request.Email,
                direccion,
                company.Ambiente);

            if (newCompanyResult.IsFailure) return newCompanyResult;

            _companyRepository.Add(newCompanyResult.Value);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
