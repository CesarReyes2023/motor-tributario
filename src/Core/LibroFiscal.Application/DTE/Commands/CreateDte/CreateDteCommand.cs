using LibroFiscal.Application.Abstractions.Messaging;
using LibroFiscal.Domain.Common.Ids;

namespace LibroFiscal.Application.DTE.Commands.CreateDte;

public sealed record CreateDteCommand(
    Guid CompanyId,
    int TipoDteId,
    int Version,
    string NumeroControlCodigo,
    string NumeroControlPuntoVenta,
    long NumeroControlCorrelativo,
    DateTimeOffset FechaEmision,
    DteEmisorDto Emisor,
    DteReceptorDto? Receptor,
    IReadOnlyList<DteLineItemDto> CuerpoDocumento,
    DteResumenDto Resumen,
    int AmbienteHaciendaId,
    int ModeloFacturacionId,
    int TipoTransmisionId) : ICommand<Guid>;

public record DteEmisorDto(
    string Nit, string Nrc, string Nombre, string NombreComercial,
    string CodigoActividad, string DescripcionActividad,
    string Telefono, string Correo,
    string CodigoEstablecimiento, string PuntoVenta,
    string Departamento, string Municipio, string ComplementoDireccion);

public record DteReceptorDto(
    string Nombre, string? Nit, string? Nrc,
    string? NombreComercial, string? CodigoActividad,
    string? Telefono, string? Correo,
    string? Departamento, string? Municipio, string? ComplementoDireccion,
    string? NumeroDocumento, string? TipoDocumento);

public record DteLineItemDto(
    int NumeroLinea, string Descripcion, decimal Cantidad,
    decimal PrecioUnitario, decimal Descuento, int TipoImpuestoId,
    string? CodigoProducto, string? UnidadMedida);

public record DteResumenDto(
    decimal TotalGravada, decimal TotalExenta, decimal TotalNoSujeta,
    decimal SubTotal, decimal TotalDescuento, decimal TotalIva,
    decimal MontoTotalOperacion, decimal TotalPagar, int CondicionOperacionId);
