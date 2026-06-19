using LibroFiscal.Domain.Common.Ids;

namespace LibroFiscal.Application.Companies.Queries.GetCompanyProfile;

public sealed record CompanyProfileDto
{
    public CompanyId Id { get; init; } = CompanyId.From(System.Guid.Empty);
    public string LegalName { get; init; } = string.Empty;
    public string TradeName { get; init; } = string.Empty;
    public string Nit { get; init; } = string.Empty;
    public string Nrc { get; init; } = string.Empty;
    public string EconomicActivityCode { get; init; } = string.Empty;
    public string EconomicActivityDescription { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    
    // Address parts
    public string Department { get; init; } = string.Empty;
    public string Municipality { get; init; } = string.Empty;
    public string AddressLine { get; init; } = string.Empty;
    
    // API Integration
    public string ApiPassword { get; init; } = string.Empty;

    public string? LogoPath { get; init; }
}
