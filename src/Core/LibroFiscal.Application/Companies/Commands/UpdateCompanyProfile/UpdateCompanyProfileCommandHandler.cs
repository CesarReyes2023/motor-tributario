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
    private readonly LibroFiscal.Application.Abstractions.Services.IEncryptionService _encryptionService;

    public UpdateCompanyProfileCommandHandler(
        IRepository<Company, CompanyId> companyRepository,
        IUnitOfWork unitOfWork,
        LibroFiscal.Application.Abstractions.Services.IEncryptionService encryptionService)
    {
        _companyRepository = companyRepository;
        _unitOfWork = unitOfWork;
        _encryptionService = encryptionService;
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

        Company? company = null;
        
        if (!string.IsNullOrEmpty(request.Id))
        {
            if (System.Guid.TryParse(request.Id, out var parsedId))
            {
                company = await _companyRepository.GetByIdAsync(new CompanyId(parsedId), cancellationToken);
            }
        }
        else
        {
            // Para mantener compatibilidad con creación de primera empresa en MVP
            var companies = await _companyRepository.FindAsync(c => c.IsActive, cancellationToken);
            company = companies.Count > 0 ? companies[0] : null;
        }

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

            var newCompany = newCompanyResult.Value;
            newCompany.UpdateApiCredentials(_encryptionService.Encrypt(request.ApiPassword));
            if (!string.IsNullOrEmpty(request.LogoPath))
            {
                newCompany.UpdateLogo(request.LogoPath);
            }

            _companyRepository.Add(newCompany);
        }
        else
        {
            company.UpdateProfile(
                request.LegalName,
                request.TradeName,
                nitResult.Value,
                nrcResult.Value,
                request.EconomicActivityCode,
                request.EconomicActivityDescription,
                request.Phone,
                request.Email,
                direccion
            );
            
            company.UpdateApiCredentials(_encryptionService.Encrypt(request.ApiPassword));
            if (request.LogoPath != null)
            {
                company.UpdateLogo(request.LogoPath);
            }

            _companyRepository.Update(company);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
