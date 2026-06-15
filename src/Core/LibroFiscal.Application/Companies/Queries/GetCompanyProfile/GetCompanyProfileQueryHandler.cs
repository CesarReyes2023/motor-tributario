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

    public GetCompanyProfileQueryHandler(IRepository<Company, CompanyId> companyRepository)
    {
        _companyRepository = companyRepository;
    }

    public async Task<Result<CompanyProfileDto>> Handle(
        GetCompanyProfileQuery request,
        CancellationToken cancellationToken)
    {
        // Get the first active company (assuming single-tenant MVP or resolved by DbContext filters)
        var companies = await _companyRepository.FindAsync(c => c.IsActive, cancellationToken);
        var company = companies.Count > 0 ? companies[0] : null;

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
            AddressLine = company.DireccionFiscal.Complemento
        };
    }
}
