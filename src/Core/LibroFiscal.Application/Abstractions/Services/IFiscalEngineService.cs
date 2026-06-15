using LibroFiscal.Domain.Common.Ids;
using LibroFiscal.SharedKernel.Results;

namespace LibroFiscal.Application.Abstractions.Services;

/// <summary>
/// Fiscal Engine service interface — the heart of the tax calculation system.
/// Defined in Application, implemented in Module.FiscalEngine.
/// </summary>
public interface IFiscalEngineService
{
    /// <summary>
    /// Validates a DTE document against current fiscal rules.
    /// </summary>
    Task<Result> ValidateDteAsync(DteId dteId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates IVA and other taxes for a set of line items.
    /// </summary>
    Task<Result<TaxCalculationResult>> CalculateTaxesAsync(
        TaxCalculationRequest request,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Request for tax calculation.
/// </summary>
public sealed record TaxCalculationRequest(
    CompanyId CompanyId,
    IReadOnlyList<TaxLineItem> Items,
    DateTimeOffset FechaEmision);

/// <summary>
/// Individual item for tax calculation.
/// </summary>
public sealed record TaxLineItem(
    string Descripcion,
    decimal Cantidad,
    decimal PrecioUnitario,
    decimal Descuento,
    string? CodigoActividad);

/// <summary>
/// Result of tax calculation.
/// </summary>
public sealed record TaxCalculationResult(
    decimal TotalGravada,
    decimal TotalExenta,
    decimal TotalNoSujeta,
    decimal TotalIva,
    decimal MontoTotalOperacion);
