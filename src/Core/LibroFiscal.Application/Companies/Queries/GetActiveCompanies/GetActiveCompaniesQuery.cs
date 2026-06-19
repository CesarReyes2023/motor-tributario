using System;
using System.Collections.Generic;
using LibroFiscal.Application.Abstractions.Messaging;
using LibroFiscal.SharedKernel.Results;

namespace LibroFiscal.Application.Companies.Queries.GetActiveCompanies;

public sealed record GetActiveCompaniesQuery : IQuery<List<CompanyDto>>;

public sealed record CompanyDto(Guid Id, string LegalName, string TradeName, string Nit, string? LogoPath);
