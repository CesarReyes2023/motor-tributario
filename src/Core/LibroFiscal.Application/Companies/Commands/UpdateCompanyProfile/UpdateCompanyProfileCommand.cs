using LibroFiscal.Application.Abstractions.Messaging;
using LibroFiscal.Domain.Common.Ids;

namespace LibroFiscal.Application.Companies.Commands.UpdateCompanyProfile;

public sealed record UpdateCompanyProfileCommand(
    string Id,
    string LegalName,
    string TradeName,
    string Nit,
    string Nrc,
    string EconomicActivityCode,
    string EconomicActivityDescription,
    string Phone,
    string Email,
    string Department,
    string Municipality,
    string AddressLine) : ICommand;
