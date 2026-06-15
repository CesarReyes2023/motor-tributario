using LibroFiscal.Domain.Common.Enumerations;
using LibroFiscal.Domain.Common.Ids;
using LibroFiscal.SharedKernel.Primitives;
using LibroFiscal.SharedKernel.Results;
using LibroFiscal.SharedKernel.Specifications;

namespace LibroFiscal.Domain.FiscalEngine.Entities;

/// <summary>
/// Fiscal Rule Aggregate Root — encapsulates a single tax rule with temporal validity.
/// Rules are composable, versionable, and independent of the DTE structure.
/// 
/// The Fiscal Engine evaluates all applicable rules for a given fiscal context,
/// applying them in order of version/priority.
/// 
/// Design: Strategy + Specification patterns.
/// Each rule carries a Specification defining WHEN it applies,
/// and parameters defining HOW it calculates.
/// </summary>
public sealed class FiscalRule : AuditableAggregateRoot<FiscalRuleId>
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public TipoImpuesto TipoImpuesto { get; private set; } = null!;
    public decimal Rate { get; private set; }
    public DateTimeOffset VigenciaDesde { get; private set; }
    public DateTimeOffset? VigenciaHasta { get; private set; }
    public int Version { get; private set; }
    public int Priority { get; private set; }
    public bool IsActive { get; private set; }

    /// <summary>
    /// JSON-serialized rule parameters for flexible configuration.
    /// Allows adding new rule dimensions without schema changes.
    /// </summary>
    public string? ParametersJson { get; private set; }

    private FiscalRule() { } // EF Core

    public static Result<FiscalRule> Create(
        string code,
        string name,
        string description,
        TipoImpuesto tipoImpuesto,
        decimal rate,
        DateTimeOffset vigenciaDesde,
        DateTimeOffset? vigenciaHasta,
        int version,
        int priority,
        string? parametersJson = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            return Error.Validation("FiscalRule.CodeEmpty", "El código de la regla fiscal es obligatorio.");

        if (rate < 0 || rate > 1)
            return Error.Validation("FiscalRule.InvalidRate", "La tasa debe estar entre 0 y 1 (e.g., 0.13 para 13%).");

        if (vigenciaHasta.HasValue && vigenciaHasta <= vigenciaDesde)
            return Error.Validation("FiscalRule.InvalidVigencia", "La fecha de fin debe ser posterior a la fecha de inicio.");

        return new FiscalRule
        {
            Id = FiscalRuleId.New(),
            Code = code,
            Name = name,
            Description = description,
            TipoImpuesto = tipoImpuesto,
            Rate = rate,
            VigenciaDesde = vigenciaDesde,
            VigenciaHasta = vigenciaHasta,
            Version = version,
            Priority = priority,
            IsActive = true,
            ParametersJson = parametersJson
        };
    }

    /// <summary>
    /// Checks if this rule is applicable at the given date.
    /// </summary>
    public bool IsApplicableAt(DateTimeOffset date)
    {
        if (!IsActive) return false;
        if (date < VigenciaDesde) return false;
        if (VigenciaHasta.HasValue && date > VigenciaHasta.Value) return false;
        return true;
    }

    public void Deactivate()
    {
        IsActive = false;
        VigenciaHasta = DateTimeOffset.UtcNow;
    }
}

/// <summary>
/// Fiscal Catalog Aggregate Root — versioned reference data from Hacienda.
/// Examples: Actividad Económica, Unidad de Medida, Tipo de Moneda, Departamentos.
/// 
/// Catalogs are loaded from versioned JSON files and stored in the database
/// for query performance and offline access.
/// </summary>
public sealed class CatalogoFiscal : AuditableAggregateRoot<CatalogoFiscalId>
{
    private readonly List<CatalogoItem> _items = [];

    public string Codigo { get; private set; } = string.Empty;
    public string Nombre { get; private set; } = string.Empty;
    public int Version { get; private set; }
    public DateTimeOffset VigenciaDesde { get; private set; }
    public DateTimeOffset? VigenciaHasta { get; private set; }
    public bool IsActive { get; private set; }

    public IReadOnlyCollection<CatalogoItem> Items => _items.AsReadOnly();

    private CatalogoFiscal() { } // EF Core

    public static CatalogoFiscal Create(
        string codigo,
        string nombre,
        int version,
        DateTimeOffset vigenciaDesde,
        IReadOnlyList<CatalogoItem> items)
    {
        var catalogo = new CatalogoFiscal
        {
            Id = CatalogoFiscalId.New(),
            Codigo = codigo,
            Nombre = nombre,
            Version = version,
            VigenciaDesde = vigenciaDesde,
            IsActive = true
        };

        catalogo._items.AddRange(items);
        return catalogo;
    }

    /// <summary>
    /// Finds an item by its code within this catalog.
    /// </summary>
    public CatalogoItem? FindByCode(string code) =>
        _items.FirstOrDefault(i => i.Codigo == code);
}

/// <summary>
/// Individual item within a fiscal catalog.
/// </summary>
public sealed class CatalogoItem : ValueObject
{
    public string Codigo { get; }
    public string Descripcion { get; }
    public bool IsActive { get; }

    public CatalogoItem(string codigo, string descripcion, bool isActive = true)
    {
        Codigo = codigo;
        Descripcion = descripcion;
        IsActive = isActive;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Codigo;
    }
}
