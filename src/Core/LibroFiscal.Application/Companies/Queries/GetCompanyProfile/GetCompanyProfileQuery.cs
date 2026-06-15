using LibroFiscal.Application.Abstractions.Messaging;

namespace LibroFiscal.Application.Companies.Queries.GetCompanyProfile;

public sealed record GetCompanyProfileQuery : IQuery<CompanyProfileDto>;
