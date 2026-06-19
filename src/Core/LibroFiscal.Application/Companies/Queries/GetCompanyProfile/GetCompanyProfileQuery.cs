using LibroFiscal.Application.Abstractions.Messaging;

namespace LibroFiscal.Application.Companies.Queries.GetCompanyProfile;

public sealed record GetCompanyProfileQuery(System.Guid CompanyId) : IQuery<CompanyProfileDto>;
