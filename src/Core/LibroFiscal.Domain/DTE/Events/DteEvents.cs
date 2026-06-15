using LibroFiscal.Domain.Common.Enumerations;
using LibroFiscal.Domain.Common.Ids;
using LibroFiscal.SharedKernel.Events;

namespace LibroFiscal.Domain.DTE.Events;

/// <summary>Raised when a new DTE is created in Borrador state.</summary>
public sealed record DteCreatedEvent(
    DteId DteId,
    CompanyId CompanyId,
    TipoDte TipoDte,
    string CodigoGeneracion) : DomainEvent;

/// <summary>Raised when a DTE passes fiscal engine validation.</summary>
public sealed record DteValidatedEvent(
    DteId DteId,
    CompanyId CompanyId) : DomainEvent;

/// <summary>
/// Raised when Hacienda accepts and seals a DTE.
/// This is the most important domain event — triggers:
/// - FiscalBooks entry creation
/// - Audit trail recording
/// - Notification to user
/// - Reporting data update
/// </summary>
public sealed record DteSealedEvent(
    DteId DteId,
    CompanyId CompanyId,
    TipoDte TipoDte,
    string CodigoGeneracion,
    string SelloRecepcion,
    DateTimeOffset FechaEmision) : DomainEvent;

/// <summary>Raised when Hacienda rejects a DTE.</summary>
public sealed record DteRejectedEvent(
    DteId DteId,
    CompanyId CompanyId,
    string Motivo,
    int IntentosTransmision) : DomainEvent;

/// <summary>Raised when a sealed DTE is annulled.</summary>
public sealed record DteAnnulledEvent(
    DteId DteId,
    CompanyId CompanyId,
    string Motivo) : DomainEvent;
