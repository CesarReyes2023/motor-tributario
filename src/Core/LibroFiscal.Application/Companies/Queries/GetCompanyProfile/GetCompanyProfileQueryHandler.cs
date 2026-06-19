using LibroFiscal.Application.Abstractions.Messaging;
using LibroFiscal.Domain.Companies.Entities;
using LibroFiscal.Domain.Common.Ids;
using LibroFiscal.SharedKernel.Interfaces;
using LibroFiscal.SharedKernel.Results;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LibroFiscal.Application.Companies.Queries.GetCompanyProfile;

internal sealed class GetCompanyProfileQueryHandler : IQueryHandler<GetCompanyProfileQuery, CompanyProfileDto>
{
    private readonly IRepository<Company, CompanyId> _companyRepository;
    private readonly LibroFiscal.Application.Abstractions.Services.IEncryptionService _encryptionService;

    public GetCompanyProfileQueryHandler(
        IRepository<Company, CompanyId> companyRepository,
        LibroFiscal.Application.Abstractions.Services.IEncryptionService encryptionService)
    {
        _companyRepository = companyRepository;
        _encryptionService = encryptionService;
    }

    public async Task<Result<CompanyProfileDto>> Handle(
        GetCompanyProfileQuery request,
        CancellationToken cancellationToken)
    {
        var company = await _companyRepository.GetByIdAsync(new CompanyId(request.CompanyId), cancellationToken);

        if (company is null)
        {
            return new CompanyProfileDto(); // Return empty DTO if not configured
        }

        return new CompanyProfileDto
        {
            Id = company.Id,
            LegalName = company.RazonSocial,
            TradeName = company.NombreComercial,
            Nit = company.Nit.Value,
            Nrc = company.Nrc.Value,
            EconomicActivityCode = company.CodigoActividad,
            EconomicActivityDescription = company.DescripcionActividad,
            Phone = company.Telefono,
            Email = company.Correo,
            Department = company.DireccionFiscal.Departamento,
            Municipality = company.DireccionFiscal.Municipio,
            AddressLine = company.DireccionFiscal.Complemento,
            ApiPassword = _encryptionService.Decrypt(company.ApiPassword),
            LogoPath = company.LogoPath
        };
    }
}
