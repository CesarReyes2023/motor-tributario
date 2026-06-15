using LibroFiscal.SharedKernel.Events;

namespace LibroFiscal.Contracts.IntegrationEvents;

/// <summary>
/// Published when a DTE is sealed by Hacienda.
/// Consumed by: FiscalBooks (to add entry), Audit (to log), Reporting (to update).
/// This crosses bounded context boundaries.
/// </summary>
public sealed record DteSealedIntegrationEvent(
    Guid DteId,
    Guid CompanyId,
    string TipoDteCodigo,
    string CodigoGeneracion,
    string SelloRecepcion,
    DateTimeOffset FechaEmision,
    decimal TotalGravada,
    decimal TotalIva,
    decimal MontoTotalOperacion) : IntegrationEvent;

/// <summary>
/// Published when a company is created.
/// Consumed by: FiscalEngine (to initialize rules), DTE (to set defaults).
/// </summary>
public sealed record CompanyCreatedIntegrationEvent(
    Guid CompanyId,
    string RazonSocial,
    string Nit) : IntegrationEvent;
