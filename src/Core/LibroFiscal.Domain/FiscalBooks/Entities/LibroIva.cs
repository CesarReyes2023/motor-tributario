#pragma warning disable CS8618 // EF Core constructor bindings

using LibroFiscal.Domain.Common.Enumerations;
using LibroFiscal.Domain.Common.Ids;
using LibroFiscal.Domain.Common.ValueObjects;
using LibroFiscal.SharedKernel.Primitives;
using LibroFiscal.SharedKernel.Results;

namespace LibroFiscal.Domain.FiscalBooks.Entities;

/// <summary>
/// Libro IVA Aggregate Root — represents a monthly IVA book for a company.
/// Types: Compras a Contribuyentes, Ventas a Contribuyentes, Ventas a Consumidor Final.
/// Generated from sealed DTEs for a given fiscal period.
/// 
/// Art. 141 Código Tributario de El Salvador.
/// </summary>
public sealed class LibroIva : AuditableAggregateRoot<LibroIvaId>
{
    private readonly List<EntradaLibro> _entradas = [];

    public CompanyId CompanyId { get; private set; } = null!;
    public TipoLibroIva TipoLibro { get; private set; } = null!;
    public FiscalPeriod Periodo { get; private set; } = null!;
    public EstadoLibroIva Estado { get; private set; } = null!;

    // ── Totals ────────────────────────────────────────────────────
    public decimal TotalGravado { get; private set; }
    public decimal TotalExento { get; private set; }
    public decimal TotalNoSujeto { get; private set; }
    public decimal TotalIva { get; private set; }
    public decimal TotalGeneral { get; private set; }

    public IReadOnlyCollection<EntradaLibro> Entradas => _entradas.AsReadOnly();

    private LibroIva() { } // EF Core

    public static Result<LibroIva> Create(
        CompanyId companyId,
        TipoLibroIva tipoLibro,
        FiscalPeriod periodo)
    {
        var libro = new LibroIva
        {
            Id = LibroIvaId.New(),
            CompanyId = companyId,
            TipoLibro = tipoLibro,
            Periodo = periodo,
            Estado = EstadoLibroIva.Borrador,
            TotalGravado = 0,
            TotalExento = 0,
            TotalNoSujeto = 0,
            TotalIva = 0,
            TotalGeneral = 0
        };

        return libro;
    }

    /// <summary>
    /// Adds a DTE entry to this book. Called when a DTE is sealed.
    /// </summary>
    public Result AddEntry(EntradaLibro entrada)
    {
        if (Estado != EstadoLibroIva.Borrador)
            return Error.Conflict("LibroIva.NotEditable", $"No se pueden agregar entradas a un libro en estado '{Estado.Name}'.");

        _entradas.Add(entrada);
        RecalculateTotals();
        return Result.Success();
    }

    /// <summary>
    /// Marks the book as generated (finalized for the period).
    /// </summary>
    public Result Generate()
    {
        if (Estado != EstadoLibroIva.Borrador)
            return Error.Conflict("LibroIva.AlreadyGenerated", "El libro ya fue generado.");

        if (_entradas.Count == 0)
            return Error.Validation("LibroIva.SinEntradas", "No se puede generar un libro sin entradas.");

        Estado = EstadoLibroIva.Generado;
        return Result.Success();
    }

    /// <summary>
    /// Marks the book as presented to Hacienda.
    /// </summary>
    public Result MarkAsPresented()
    {
        if (Estado != EstadoLibroIva.Generado)
            return Error.Conflict("LibroIva.NotGenerated", "El libro debe estar generado antes de presentarlo.");

        Estado = EstadoLibroIva.Presentado;
        return Result.Success();
    }

    private void RecalculateTotals()
    {
        TotalGravado = _entradas.Sum(e => e.Gravado);
        TotalExento = _entradas.Sum(e => e.Exento);
        TotalNoSujeto = _entradas.Sum(e => e.NoSujeto);
        TotalIva = _entradas.Sum(e => e.Iva);
        TotalGeneral = TotalGravado + TotalExento + TotalNoSujeto;
    }
}

/// <summary>
/// Individual entry in an IVA book, corresponding to a sealed DTE.
/// </summary>
public sealed class EntradaLibro : ValueObject
{
    public int Correlativo { get; }
    public DateTimeOffset FechaEmision { get; }
    public string NumeroControl { get; }
    public string CodigoGeneracion { get; }
    public string NombreProveedor { get; }
    public string? NitProveedor { get; }
    public string? NrcProveedor { get; }
    public decimal Gravado { get; }
    public decimal Exento { get; }
    public decimal NoSujeto { get; }
    public decimal Iva { get; }
    public decimal Total { get; }
    public DteId DteId { get; }

    private EntradaLibro() { } // EF Core

    public EntradaLibro(
        int correlativo,
        DateTimeOffset fechaEmision,
        string numeroControl,
        string codigoGeneracion,
        string nombreProveedor,
        string? nitProveedor,
        string? nrcProveedor,
        decimal gravado,
        decimal exento,
        decimal noSujeto,
        decimal iva,
        DteId dteId)
    {
        Correlativo = correlativo;
        FechaEmision = fechaEmision;
        NumeroControl = numeroControl;
        CodigoGeneracion = codigoGeneracion;
        NombreProveedor = nombreProveedor;
        NitProveedor = nitProveedor;
        NrcProveedor = nrcProveedor;
        Gravado = gravado;
        Exento = exento;
        NoSujeto = noSujeto;
        Iva = iva;
        Total = gravado + exento + noSujeto;
        DteId = dteId;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return CodigoGeneracion;
        yield return DteId;
    }
}
