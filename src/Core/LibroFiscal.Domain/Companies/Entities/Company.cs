using LibroFiscal.Domain.Common.Enumerations;
using LibroFiscal.Domain.Common.Ids;
using LibroFiscal.Domain.Common.ValueObjects;
using LibroFiscal.SharedKernel.Primitives;
using LibroFiscal.SharedKernel.Results;

namespace LibroFiscal.Domain.Companies.Entities;

/// <summary>
/// Company Aggregate Root — represents a fiscal entity (contribuyente) in El Salvador.
/// Each company has its own fiscal identity, establishments, and configuration.
/// This is the multi-tenancy anchor — all other aggregates reference CompanyId.
/// </summary>
public sealed class Company : AuditableAggregateRoot<CompanyId>
{
    private readonly List<Establishment> _establishments = [];

    public string RazonSocial { get; private set; } = string.Empty;
    public string NombreComercial { get; private set; } = string.Empty;
    public Nit Nit { get; private set; } = null!;
    public Nrc Nrc { get; private set; } = null!;
    public string CodigoActividad { get; private set; } = string.Empty;
    public string DescripcionActividad { get; private set; } = string.Empty;
    public string Telefono { get; private set; } = string.Empty;
    public string Correo { get; private set; } = string.Empty;
    public DireccionFiscal DireccionFiscal { get; private set; } = null!;
    public AmbienteHacienda Ambiente { get; private set; } = null!;
    public string ApiPassword { get; private set; } = string.Empty;
    public string? LogoPath { get; private set; }
    public bool IsActive { get; private set; }

    /// <summary>Establishments (sucursales/puntos de venta) of this company.</summary>
    public IReadOnlyCollection<Establishment> Establishments => _establishments.AsReadOnly();

    private Company() { } // EF Core

    public static Result<Company> Create(
        string razonSocial,
        string nombreComercial,
        Nit nit,
        Nrc nrc,
        string codigoActividad,
        string descripcionActividad,
        string telefono,
        string correo,
        DireccionFiscal direccionFiscal,
        AmbienteHacienda ambiente,
        string? logoPath = null)
    {
        if (string.IsNullOrWhiteSpace(razonSocial))
            return Error.Validation("Company.RazonSocialVacia", "La razón social es obligatoria.");

        var company = new Company
        {
            Id = CompanyId.New(),
            RazonSocial = razonSocial,
            NombreComercial = nombreComercial,
            Nit = nit,
            Nrc = nrc,
            CodigoActividad = codigoActividad,
            DescripcionActividad = descripcionActividad,
            Telefono = telefono,
            Correo = correo,
            DireccionFiscal = direccionFiscal,
            Ambiente = ambiente,
            LogoPath = logoPath,
            IsActive = true
        };

        return company;
    }

    public Result<Establishment> AddEstablishment(
        string codigo,
        string nombre,
        string puntoVenta,
        DireccionFiscal direccion,
        string telefono,
        string correo)
    {
        if (_establishments.Any(e => e.Codigo == codigo))
            return Error.Conflict("Company.EstablishmentDuplicate", $"Ya existe un establecimiento con código '{codigo}'.");

        var establishment = Establishment.Create(
            Id, codigo, nombre, puntoVenta, direccion, telefono, correo);

        _establishments.Add(establishment);
        return establishment;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void UpdateFiscalConfig(AmbienteHacienda ambiente)
    {
        Ambiente = ambiente;
    }

    public void UpdateApiCredentials(string apiPassword)
    {
        ApiPassword = apiPassword;
    }
    
    public void UpdateLogo(string? logoPath)
    {
        LogoPath = logoPath;
    }

    public void UpdateProfile(
        string razonSocial,
        string nombreComercial,
        Nit nit,
        Nrc nrc,
        string codigoActividad,
        string descripcionActividad,
        string telefono,
        string correo,
        DireccionFiscal direccionFiscal)
    {
        RazonSocial = razonSocial;
        NombreComercial = nombreComercial;
        Nit = nit;
        Nrc = nrc;
        CodigoActividad = codigoActividad;
        DescripcionActividad = descripcionActividad;
        Telefono = telefono;
        Correo = correo;
        DireccionFiscal = direccionFiscal;
    }
}

/// <summary>
/// Establishment (Sucursal / Punto de Venta) belonging to a Company.
/// Each establishment has its own code and can issue DTEs independently.
/// </summary>
public sealed class Establishment : Entity<EstablishmentId>
{
    public CompanyId CompanyId { get; private set; } = null!;
    public string Codigo { get; private set; } = string.Empty;
    public string Nombre { get; private set; } = string.Empty;
    public string PuntoVenta { get; private set; } = string.Empty;
    public DireccionFiscal Direccion { get; private set; } = null!;
    public string Telefono { get; private set; } = string.Empty;
    public string Correo { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    private Establishment() { } // EF Core

    internal static Establishment Create(
        CompanyId companyId,
        string codigo,
        string nombre,
        string puntoVenta,
        DireccionFiscal direccion,
        string telefono,
        string correo)
    {
        return new Establishment
        {
            Id = EstablishmentId.New(),
            CompanyId = companyId,
            Codigo = codigo,
            Nombre = nombre,
            PuntoVenta = puntoVenta,
            Direccion = direccion,
            Telefono = telefono,
            Correo = correo,
            IsActive = true
        };
    }
}
