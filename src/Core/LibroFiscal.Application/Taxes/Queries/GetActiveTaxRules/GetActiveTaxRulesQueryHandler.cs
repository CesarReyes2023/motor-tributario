using LibroFiscal.Application.Abstractions.Messaging;
using LibroFiscal.SharedKernel.Interfaces;
using LibroFiscal.SharedKernel.Results;
using LibroFiscal.Domain.Taxes.Entities;

namespace LibroFiscal.Application.Taxes.Queries.GetActiveTaxRules;

public sealed class GetActiveTaxRulesQueryHandler : IQueryHandler<GetActiveTaxRulesQuery, IReadOnlyList<TaxRuleDto>>
{
    private readonly IRepository<TaxRule, Guid> _taxRuleRepository;

    public GetActiveTaxRulesQueryHandler(IRepository<TaxRule, Guid> taxRuleRepository)
    {
        _taxRuleRepository = taxRuleRepository;
    }

    public async Task<Result<IReadOnlyList<TaxRuleDto>>> Handle(GetActiveTaxRulesQuery request, CancellationToken cancellationToken)
    {
        var rules = await _taxRuleRepository.FindAsync(x => x.IsActive, cancellationToken);

        var dtos = rules.Select(x => new TaxRuleDto(
            x.Id,
            x.Name,
            x.Code,
            x.Rate,
            x.Type.Id
        )).ToList();

        return Result.Success<IReadOnlyList<TaxRuleDto>>(dtos);
    }
}
