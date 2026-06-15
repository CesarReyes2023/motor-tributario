using LibroFiscal.Application.Abstractions.Messaging;
using LibroFiscal.Domain.Taxes.Services;
using LibroFiscal.SharedKernel.Interfaces;
using LibroFiscal.SharedKernel.Results;
using LibroFiscal.Domain.Taxes.Entities;

namespace LibroFiscal.Application.Taxes.Queries.CalculateDteTaxes;

public sealed class CalculateDteTaxesQueryHandler : IQueryHandler<CalculateDteTaxesQuery, TaxCalculationResultDto>
{
    private readonly ITaxEngine _taxEngine;
    private readonly IRepository<TaxRule, Guid> _taxRuleRepository;

    public CalculateDteTaxesQueryHandler(ITaxEngine taxEngine, IRepository<TaxRule, Guid> taxRuleRepository)
    {
        _taxEngine = taxEngine;
        _taxRuleRepository = taxRuleRepository;
    }

    public async Task<Result<TaxCalculationResultDto>> Handle(CalculateDteTaxesQuery request, CancellationToken cancellationToken)
    {
        // 1. Obtener todas las reglas de impuestos activas
        var activeRules = await _taxRuleRepository.FindAsync(x => x.IsActive, cancellationToken);

        // 2. Ejecutar el Domain Service para calcular
        var calculationResult = _taxEngine.Calculate(request.Subtotal, activeRules);

        // 3. Mapear a DTO
        var dto = new TaxCalculationResultDto(
            calculationResult.Subtotal,
            calculationResult.TotalTaxes,
            calculationResult.TotalDeductions,
            calculationResult.GrandTotal,
            calculationResult.TaxDetails
        );

        return Result.Success(dto);
    }
}
