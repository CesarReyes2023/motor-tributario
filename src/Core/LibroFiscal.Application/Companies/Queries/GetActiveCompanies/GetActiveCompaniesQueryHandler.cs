using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LibroFiscal.Application.Abstractions.Messaging;
using LibroFiscal.Domain.Common.Ids;
using LibroFiscal.Domain.Companies.Entities;
using LibroFiscal.SharedKernel.Interfaces;
using LibroFiscal.SharedKernel.Results;

namespace LibroFiscal.Application.Companies.Queries.GetActiveCompanies;

internal sealed class GetActiveCompaniesQueryHandler : IQueryHandler<GetActiveCompaniesQuery, List<CompanyDto>>
{
    private readonly IRepository<Company, CompanyId> _companyRepository;

    public GetActiveCompaniesQueryHandler(IRepository<Company, CompanyId> companyRepository)
    {
        _companyRepository = companyRepository;
    }

    public async Task<Result<List<CompanyDto>>> Handle(GetActiveCompaniesQuery request, CancellationToken cancellationToken)
    {
        var companies = await _companyRepository.FindAsync(c => c.IsActive, cancellationToken);

        var dtoList = companies.Select(c => new CompanyDto(
            c.Id.Value,
            c.RazonSocial,
            c.NombreComercial,
            c.Nit.Value,
            c.LogoPath
        )).ToList();

        return dtoList;
    }
}
