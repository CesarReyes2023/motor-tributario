#pragma warning disable CS8618 // EF Core constructor bindings

using LibroFiscal.Domain.Common.Enumerations;
using LibroFiscal.Domain.Common.Ids;
using LibroFiscal.Domain.Common.ValueObjects;
using LibroFiscal.Domain.DTE.Events;
using LibroFiscal.SharedKernel.Primitives;
using LibroFiscal.SharedKernel.Results;

namespace LibroFiscal.Domain.DTE.Entities;

/// <summary>
/// DTE Document Aggregate Root — the core entity of the fiscal platform.
/// Manages the complete lifecycle of a Documento Tributario Electrónico:
/// Borrador → Validado → Firmado → EnCola → Transmitiendo → Sellado/Rechazado/Anulado
/// 
/// Enforces all state transition invariants. External code cannot set estado directly —
/// must go through domain methods that validate transitions.
/// </summary>
public sealed class DteDocument : AuditableAggregateRoot<DteId>
{
    private readonly List<DteLineItem> _cuerpoDocumento = [];
    private readonly List<DteStatusChange> _historialEstados = [];

    // ── Identity ──────────────────────────────────────────────────
    public CompanyId CompanyId { get; private set; } = null!;
    public TipoDte TipoDte { get; private set; } = null!;
    public int Version { get; private set; }

    /// <summary>UUID v4 — unique identifier assigned by the emisor system.</summary>
    public string CodigoGeneracion { get; private set; } = string.Empty;

    /// <summary>Structured control number: DTE-{Tipo}-{Est}-{PV}-{Corr}</summary>
    public NumeroControl NumeroControl { get; private set; } = null!;

    // ── Temporal ──────────────────────────────────────────────────
    public DateTimeOffset FechaEmision { get; private set; }
    public DateTimeOffset? FechaTransmision { get; private set; }
    public DateTimeOffset? FechaAnulacion { get; private set; }

    // ── Participants ──────────────────────────────────────────────
    public DteEmisor Emisor { get; private set; } = null!;
    public DteReceptor? Receptor { get; private set; }

    // ── Content ───────────────────────────────────────────────────
    public IReadOnlyCollection<DteLineItem> CuerpoDocumento => _cuerpoDocumento.AsReadOnly();
    public DteResumen Resumen { get; private set; } = null!;

    // ── State ─────────────────────────────────────────────────────
    public EstadoDte Estado { get; private set; } = null!;
    public IReadOnlyCollection<DteStatusChange> HistorialEstados => _historialEstados.AsReadOnly();

    // ── Hacienda Response ─────────────────────────────────────────
    public string? SelloRecepcion { get; private set; }
    public string? FirmaElectronica { get; private set; }
    public string? JsonOriginal { get; private set; }
    public string? JsonFirmado { get; private set; }
    public string? MotivoRechazo { get; private set; }
    public int IntentosTransmision { get; private set; }

    // ── Environment ───────────────────────────────────────────────
    public AmbienteHacienda Ambiente { get; private set; } = null!;
    public ModeloFacturacion ModeloFacturacion { get; private set; } = null!;
    public TipoTransmision TipoTransmision { get; private set; } = null!;

    private DteDocument() { } // EF Core

    // ══════════════════════════════════════════════════════════════
    // Factory Method — single entry point for creating DTEs
    // ══════════════════════════════════════════════════════════════

    public static Result<DteDocument> Create(
        CompanyId companyId,
        TipoDte tipoDte,
        int version,
        NumeroControl numeroControl,
        DateTimeOffset fechaEmision,
        DteEmisor emisor,
        DteReceptor? receptor,
        IReadOnlyList<DteLineItem> cuerpoDocumento,
        DteResumen resumen,
        AmbienteHacienda ambiente,
        ModeloFacturacion modeloFacturacion,
        TipoTransmision tipoTransmision)
    {
        if (cuerpoDocumento.Count == 0)
            return Error.Validation("DTE.CuerpoVacio", "El DTE debe contener al menos una línea en el cuerpo del documento.");

        var dte = new DteDocument
        {
            Id = DteId.New(),
            CompanyId = companyId,
            TipoDte = tipoDte,
            Version = version,
            CodigoGeneracion = Guid.NewGuid().ToString(),
            NumeroControl = numeroControl,
            FechaEmision = fechaEmision,
            Emisor = emisor,
            Receptor = receptor,
            Resumen = resumen,
            Estado = EstadoDte.Borrador,
            Ambiente = ambiente,
            ModeloFacturacion = modeloFacturacion,
            TipoTransmision = tipoTransmision,
            IntentosTransmision = 0
        };

        dte._cuerpoDocumento.AddRange(cuerpoDocumento);
        dte.RecordStatusChange(EstadoDte.Borrador, "DTE creado");
        dte.RaiseDomainEvent(new DteCreatedEvent(dte.Id, dte.CompanyId, dte.TipoDte, dte.CodigoGeneracion));

        return dte;
    }

    // ══════════════════════════════════════════════════════════════
    // State Transitions — enforce invariants
    // ══════════════════════════════════════════════════════════════

    public Result MarkAsValidated()
    {
        if (Estado != EstadoDte.Borrador)
            return Error.Conflict("DTE.InvalidTransition", $"No se puede validar un DTE en estado '{Estado.Name}'. Debe estar en 'Borrador'.");

        Estado = EstadoDte.Validado;
        RecordStatusChange(EstadoDte.Validado, "DTE validado por el motor fiscal");
        RaiseDomainEvent(new DteValidatedEvent(Id, CompanyId));
        return Result.Success();
    }

    public Result MarkAsSigned(string firmaElectronica, string jsonOriginal, string jsonFirmado)
    {
        if (Estado != EstadoDte.Validado)
            return Error.Conflict("DTE.InvalidTransition", $"No se puede firmar un DTE en estado '{Estado.Name}'. Debe estar en 'Validado'.");

        if (string.IsNullOrWhiteSpace(firmaElectronica))
            return Error.Validation("DTE.FirmaVacia", "La firma electrónica no puede estar vacía.");

        FirmaElectronica = firmaElectronica;
        JsonOriginal = jsonOriginal;
        JsonFirmado = jsonFirmado;
        Estado = EstadoDte.Firmado;
        RecordStatusChange(EstadoDte.Firmado, "DTE firmado electrónicamente (JWS)");
        return Result.Success();
    }

    public Result EnqueueForTransmission()
    {
        if (Estado != EstadoDte.Firmado && Estado != EstadoDte.Rechazado)
            return Error.Conflict("DTE.InvalidTransition", $"No se puede encolar un DTE en estado '{Estado.Name}'. Debe estar en 'Firmado' o 'Rechazado'.");

        Estado = EstadoDte.EnCola;
        RecordStatusChange(EstadoDte.EnCola, "DTE encolado para transmisión asíncrona");
        return Result.Success();
    }

    public Result MarkAsTransmitting()
    {
        if (Estado != EstadoDte.EnCola)
            return Error.Conflict("DTE.InvalidTransition", $"No se puede transmitir un DTE en estado '{Estado.Name}'. Debe estar 'En Cola'.");

        IntentosTransmision++;
        Estado = EstadoDte.Transmitiendo;
        RecordStatusChange(EstadoDte.Transmitiendo, $"Intento de transmisión #{IntentosTransmision}");
        return Result.Success();
    }

    public Result MarkAsSealed(string selloRecepcion, DateTimeOffset fechaTransmision)
    {
        if (Estado != EstadoDte.Transmitiendo)
            return Error.Conflict("DTE.InvalidTransition", $"No se puede sellar un DTE en estado '{Estado.Name}'. Debe estar 'Transmitiendo'.");

        if (string.IsNullOrWhiteSpace(selloRecepcion))
            return Error.Validation("DTE.SelloVacio", "El sello de recepción de Hacienda no puede estar vacío.");

        SelloRecepcion = selloRecepcion;
        FechaTransmision = fechaTransmision;
        Estado = EstadoDte.Sellado;
        RecordStatusChange(EstadoDte.Sellado, $"DTE sellado por Hacienda. Sello: {selloRecepcion}");
        RaiseDomainEvent(new DteSealedEvent(Id, CompanyId, TipoDte, CodigoGeneracion, selloRecepcion, FechaEmision));
        return Result.Success();
    }

    public Result MarkAsRejected(string motivo)
    {
        if (Estado != EstadoDte.Transmitiendo)
            return Error.Conflict("DTE.InvalidTransition", $"No se puede rechazar un DTE en estado '{Estado.Name}'.");

        MotivoRechazo = motivo;
        Estado = EstadoDte.Rechazado;
        RecordStatusChange(EstadoDte.Rechazado, $"Rechazado por Hacienda: {motivo}");
        RaiseDomainEvent(new DteRejectedEvent(Id, CompanyId, motivo, IntentosTransmision));
        return Result.Success();
    }

    public Result MarkAsFinalError(string motivo)
    {
        if (Estado != EstadoDte.Rechazado)
            return Error.Conflict("DTE.InvalidTransition", "Solo DTEs rechazados pueden pasar a error final.");

        Estado = EstadoDte.ErrorFinal;
        RecordStatusChange(EstadoDte.ErrorFinal, $"Error final después de {IntentosTransmision} intentos: {motivo}");
        return Result.Success();
    }

    public Result Anular(string motivo)
    {
        if (Estado != EstadoDte.Sellado)
            return Error.Conflict("DTE.InvalidTransition", "Solo DTEs sellados pueden ser anulados.");

        FechaAnulacion = DateTimeOffset.UtcNow;
        Estado = EstadoDte.Anulado;
        RecordStatusChange(EstadoDte.Anulado, $"Anulado: {motivo}");
        RaiseDomainEvent(new DteAnnulledEvent(Id, CompanyId, motivo));
        return Result.Success();
    }

    // ══════════════════════════════════════════════════════════════
    // Queries
    // ══════════════════════════════════════════════════════════════

    /// <summary>Whether this DTE has full fiscal validity (sealed by Hacienda).</summary>
    public bool HasFiscalValidity => Estado == EstadoDte.Sellado;

    /// <summary>Whether this DTE can be retried after rejection.</summary>
    public bool CanRetry => Estado == EstadoDte.Rechazado;

    // ══════════════════════════════════════════════════════════════
    // Private
    // ══════════════════════════════════════════════════════════════

    private void RecordStatusChange(EstadoDte newEstado, string descripcion)
    {
        _historialEstados.Add(new DteStatusChange(
            newEstado,
            DateTimeOffset.UtcNow,
            descripcion));
    }
}

/// <summary>
/// Line item within a DTE document body.
/// Represents a single product/service with its tax classification.
/// </summary>
public sealed class DteLineItem : ValueObject
{
    public int NumeroLinea { get; private set; }
    public string Descripcion { get; private set; }
    public decimal Cantidad { get; private set; }
    public decimal PrecioUnitario { get; private set; }
    public decimal Descuento { get; private set; }
    public decimal SubTotal { get; private set; }
    public decimal IvaItem { get; private set; }
    public string? CodigoProducto { get; private set; }
    public string? UnidadMedida { get; private set; }
    public TipoImpuesto TipoImpuesto { get; private set; }

    private DteLineItem() { } // EF Core

    public DteLineItem(
        int numeroLinea,
        string descripcion,
        decimal cantidad,
        decimal precioUnitario,
        decimal descuento,
        TipoImpuesto tipoImpuesto,
        string? codigoProducto = null,
        string? unidadMedida = null)
    {
        NumeroLinea = numeroLinea;
        Descripcion = descripcion;
        Cantidad = cantidad;
        PrecioUnitario = precioUnitario;
        Descuento = descuento;
        TipoImpuesto = tipoImpuesto;
        CodigoProducto = codigoProducto;
        UnidadMedida = unidadMedida;
        SubTotal = Math.Round(cantidad * precioUnitario - descuento, 2);
        IvaItem = tipoImpuesto == TipoImpuesto.Iva ? Math.Round(SubTotal * 0.13m, 2) : 0m;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return NumeroLinea;
        yield return Descripcion;
        yield return Cantidad;
        yield return PrecioUnitario;
    }
}

/// <summary>
/// Emisor data embedded in a DTE. Snapshot of company fiscal data at emission time.
/// </summary>
public sealed class DteEmisor : ValueObject
{
    public string Nit { get; private set; }
    public string Nrc { get; private set; }
    public string Nombre { get; private set; }
    public string NombreComercial { get; private set; }
    public string CodigoActividad { get; private set; }
    public string DescripcionActividad { get; private set; }
    public string Telefono { get; private set; }
    public string Correo { get; private set; }
    public string CodigoEstablecimiento { get; private set; }
    public string PuntoVenta { get; private set; }
    public DireccionFiscal Direccion { get; private set; }

    private DteEmisor() { } // EF Core

    public DteEmisor(
        string nit, string nrc, string nombre, string nombreComercial,
        string codigoActividad, string descripcionActividad,
        string telefono, string correo,
        string codigoEstablecimiento, string puntoVenta,
        DireccionFiscal direccion)
    {
        Nit = nit;
        Nrc = nrc;
        Nombre = nombre;
        NombreComercial = nombreComercial;
        CodigoActividad = codigoActividad;
        DescripcionActividad = descripcionActividad;
        Telefono = telefono;
        Correo = correo;
        CodigoEstablecimiento = codigoEstablecimiento;
        PuntoVenta = puntoVenta;
        Direccion = direccion;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Nit;
        yield return Nrc;
        yield return Nombre;
    }
}

/// <summary>
/// Receptor data embedded in a DTE. May be null for certain document types.
/// </summary>
public sealed class DteReceptor : ValueObject
{
    public string? Nit { get; private set; }
    public string? Nrc { get; private set; }
    public string Nombre { get; private set; }
    public string? NombreComercial { get; private set; }
    public string? CodigoActividad { get; private set; }
    public string? Telefono { get; private set; }
    public string? Correo { get; private set; }
    public DireccionFiscal? Direccion { get; private set; }
    public string? NumeroDocumento { get; private set; }
    public string? TipoDocumento { get; private set; }

    private DteReceptor() { } // EF Core

    public DteReceptor(
        string nombre,
        string? nit = null, string? nrc = null,
        string? nombreComercial = null, string? codigoActividad = null,
        string? telefono = null, string? correo = null,
        DireccionFiscal? direccion = null,
        string? numeroDocumento = null, string? tipoDocumento = null)
    {
        Nombre = nombre;
        Nit = nit;
        Nrc = nrc;
        NombreComercial = nombreComercial;
        CodigoActividad = codigoActividad;
        Telefono = telefono;
        Correo = correo;
        Direccion = direccion;
        NumeroDocumento = numeroDocumento;
        TipoDocumento = tipoDocumento;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Nombre;
        yield return Nit;
        yield return NumeroDocumento;
    }
}

/// <summary>
/// DTE summary section — totals, taxes, payments.
/// </summary>
public sealed class DteResumen : ValueObject
{
    public decimal TotalGravada { get; private set; }
    public decimal TotalExenta { get; private set; }
    public decimal TotalNoSujeta { get; private set; }
    public decimal SubTotal { get; private set; }
    public decimal TotalDescuento { get; private set; }
    public decimal TotalIva { get; private set; }
    public decimal MontoTotalOperacion { get; private set; }
    public decimal TotalPagar { get; private set; }
    public CondicionOperacion CondicionOperacion { get; private set; }

    private DteResumen() { } // EF Core

    public DteResumen(
        decimal totalGravada,
        decimal totalExenta,
        decimal totalNoSujeta,
        decimal subTotal,
        decimal totalDescuento,
        decimal totalIva,
        decimal montoTotalOperacion,
        decimal totalPagar,
        CondicionOperacion condicionOperacion)
    {
        TotalGravada = totalGravada;
        TotalExenta = totalExenta;
        TotalNoSujeta = totalNoSujeta;
        SubTotal = subTotal;
        TotalDescuento = totalDescuento;
        TotalIva = totalIva;
        MontoTotalOperacion = montoTotalOperacion;
        TotalPagar = totalPagar;
        CondicionOperacion = condicionOperacion;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return MontoTotalOperacion;
        yield return TotalIva;
        yield return TotalPagar;
    }
}

/// <summary>
/// Records a state transition in the DTE lifecycle for audit purposes.
/// </summary>
public sealed class DteStatusChange : ValueObject
{
    public EstadoDte Estado { get; }
    public DateTimeOffset Timestamp { get; }
    public string Descripcion { get; }

    private DteStatusChange() { } // EF Core

    public DteStatusChange(EstadoDte estado, DateTimeOffset timestamp, string descripcion)
    {
        Estado = estado;
        Timestamp = timestamp;
        Descripcion = descripcion;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Estado;
        yield return Timestamp;
    }
}
