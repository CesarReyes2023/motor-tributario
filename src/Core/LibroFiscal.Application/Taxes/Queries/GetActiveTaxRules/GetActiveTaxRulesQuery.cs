using LibroFiscal.Application.Abstractions.Messaging;

namespace LibroFiscal.Application.Taxes.Queries.GetActiveTaxRules;

public sealed record GetActiveTaxRulesQuery : IQuery<IReadOnlyList<TaxRuleDto>>;

public sealed record TaxRuleDto(
    Guid Id,
    string Name,
    string Code,
    decimal Rate,
    int Type
);
