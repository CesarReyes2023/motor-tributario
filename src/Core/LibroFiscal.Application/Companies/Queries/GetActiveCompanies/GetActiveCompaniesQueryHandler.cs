using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LibroFiscal.Application.Abstractions.Messaging;
using LibroFiscal.Application.Abstractions.Services;
using LibroFiscal.Domain.Common.Ids;
using LibroFiscal.Domain.Companies.Entities;
using LibroFiscal.Domain.Users.Entities;
using LibroFiscal.Domain.Users.Ids;
using LibroFiscal.SharedKernel.Interfaces;
using LibroFiscal.SharedKernel.Results;

namespace LibroFiscal.Application.Companies.Queries.GetActiveCompanies;

internal sealed class GetActiveCompaniesQueryHandler : IQueryHandler<GetActiveCompaniesQuery, List<CompanyDto>>
{
    private readonly IRepository<Company, CompanyId> _companyRepository;
    private readonly IRepository<User, UserId> _userRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetActiveCompaniesQueryHandler(
        IRepository<Company, CompanyId> companyRepository,
        IRepository<User, UserId> userRepository,
        ICurrentUserService currentUserService)
    {
        _companyRepository = companyRepository;
        _userRepository = userRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<CompanyDto>>> Handle(GetActiveCompaniesQuery request, CancellationToken cancellationToken)
    {
        var companies = await _companyRepository.FindAsync(c => c.IsActive, cancellationToken);

        if (_currentUserService.Role != "Admin" && _currentUserService.UserId.HasValue)
        {
            var user = await _userRepository.GetByIdAsync(new UserId(_currentUserService.UserId.Value), cancellationToken);
            if (user != null)
            {
                var allowedCompanyIds = user.CompanyAccesses.Select(ca => ca.CompanyId).ToHashSet();
                companies = companies.Where(c => allowedCompanyIds.Contains(c.Id)).ToList();
            }
            else
            {
                companies = new List<Company>();
            }
        }

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
