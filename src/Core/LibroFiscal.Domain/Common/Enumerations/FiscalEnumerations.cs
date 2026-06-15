using LibroFiscal.SharedKernel.Primitives;

namespace LibroFiscal.Domain.Common.Enumerations;

/// <summary>
/// Tipos de Documentos Tributarios Electrónicos según catálogo oficial de Hacienda El Salvador.
/// Código y estructura definidos por DGII.
/// </summary>
public sealed class TipoDte : Enumeration
{
    /// <summary>01 - Factura (Consumidor Final)</summary>
    public static readonly TipoDte Factura = new(1, "Factura", "01");

    /// <summary>03 - Comprobante de Crédito Fiscal</summary>
    public static readonly TipoDte CreditoFiscal = new(3, "Crédito Fiscal", "03");

    /// <summary>04 - Nota de Remisión</summary>
    public static readonly TipoDte NotaRemision = new(4, "Nota de Remisión", "04");

    /// <summary>05 - Nota de Crédito</summary>
    public static readonly TipoDte NotaCredito = new(5, "Nota de Crédito", "05");

    /// <summary>06 - Nota de Débito</summary>
    public static readonly TipoDte NotaDebito = new(6, "Nota de Débito", "06");

    /// <summary>07 - Comprobante de Retención</summary>
    public static readonly TipoDte ComprobanteRetencion = new(7, "Comprobante de Retención", "07");

    /// <summary>08 - Comprobante de Liquidación</summary>
    public static readonly TipoDte ComprobanteLiquidacion = new(8, "Comprobante de Liquidación", "08");

    /// <summary>09 - Documento Contable de Liquidación</summary>
    public static readonly TipoDte DocumentoContableLiquidacion = new(9, "Documento Contable de Liquidación", "09");

    /// <summary>11 - Factura de Exportación</summary>
    public static readonly TipoDte FacturaExportacion = new(11, "Factura de Exportación", "11");

    /// <summary>14 - Factura de Sujeto Excluido</summary>
    public static readonly TipoDte FacturaSujetoExcluido = new(14, "Factura de Sujeto Excluido", "14");

    /// <summary>15 - Comprobante de Donación</summary>
    public static readonly TipoDte ComprobanteDonacion = new(15, "Comprobante de Donación", "15");

    /// <summary>
    /// Código oficial de Hacienda (e.g., "01", "03", "14").
    /// </summary>
    public string Codigo { get; }

    private TipoDte(int id, string name, string codigo) : base(id, name)
    {
        Codigo = codigo;
    }

    /// <summary>
    /// Finds TipoDte by its Hacienda code (e.g., "01", "03").
    /// </summary>
    public static TipoDte FromCodigo(string codigo)
    {
        var match = GetAll<TipoDte>().FirstOrDefault(t => t.Codigo == codigo);
        return match ?? throw new InvalidOperationException(
            $"'{codigo}' is not a valid TipoDte code. Valid codes: {string.Join(", ", GetAll<TipoDte>().Select(t => t.Codigo))}");
    }
}

/// <summary>
/// Estado del ciclo de vida de un DTE.
/// Implementa el state machine: Borrador → Validado → Firmado → EnCola → Transmitiendo → Sellado/Rechazado.
/// </summary>
public sealed class EstadoDte : Enumeration
{
    public static readonly EstadoDte Borrador = new(1, "Borrador");
    public static readonly EstadoDte Validado = new(2, "Validado");
    public static readonly EstadoDte Firmado = new(3, "Firmado");
    public static readonly EstadoDte EnCola = new(4, "En Cola");
    public static readonly EstadoDte Transmitiendo = new(5, "Transmitiendo");
    public static readonly EstadoDte Sellado = new(6, "Sellado");
    public static readonly EstadoDte Rechazado = new(7, "Rechazado");
    public static readonly EstadoDte ErrorFinal = new(8, "Error Final");
    public static readonly EstadoDte Anulado = new(9, "Anulado");
    public static readonly EstadoDte Contingencia = new(10, "Contingencia");

    private EstadoDte(int id, string name) : base(id, name) { }
}

/// <summary>
/// Tipo de Libro de IVA según Art. 141 Código Tributario de El Salvador.
/// </summary>
public sealed class TipoLibroIva : Enumeration
{
    public static readonly TipoLibroIva ComprasContribuyentes = new(1, "Compras a Contribuyentes");
    public static readonly TipoLibroIva VentasContribuyentes = new(2, "Ventas a Contribuyentes");
    public static readonly TipoLibroIva VentasConsumidorFinal = new(3, "Ventas a Consumidor Final");

    private TipoLibroIva(int id, string name) : base(id, name) { }
}

/// <summary>
/// Estado del Libro IVA en su ciclo de gestión.
/// </summary>
public sealed class EstadoLibroIva : Enumeration
{
    public static readonly EstadoLibroIva Borrador = new(1, "Borrador");
    public static readonly EstadoLibroIva Generado = new(2, "Generado");
    public static readonly EstadoLibroIva Presentado = new(3, "Presentado");

    private EstadoLibroIva(int id, string name) : base(id, name) { }
}

/// <summary>
/// Tipo de impuesto aplicable en El Salvador.
/// </summary>
public sealed class TipoImpuesto : Enumeration
{
    public static readonly TipoImpuesto Iva = new(1, "IVA");
    public static readonly TipoImpuesto Retencion = new(2, "Retención");
    public static readonly TipoImpuesto Percepcion = new(3, "Percepción");
    public static readonly TipoImpuesto IvaExento = new(4, "IVA Exento");
    public static readonly TipoImpuesto NoSujeto = new(5, "No Sujeto");

    private TipoImpuesto(int id, string name) : base(id, name) { }
}

/// <summary>
/// Condición de la operación de venta/compra.
/// </summary>
public sealed class CondicionOperacion : Enumeration
{
    public static readonly CondicionOperacion Contado = new(1, "Contado");
    public static readonly CondicionOperacion Credito = new(2, "A Crédito");
    public static readonly CondicionOperacion Otro = new(3, "Otro");

    private CondicionOperacion(int id, string name) : base(id, name) { }
}

/// <summary>
/// Ambiente de operación para Hacienda API.
/// </summary>
public sealed class AmbienteHacienda : Enumeration
{
    public static readonly AmbienteHacienda Pruebas = new(0, "Pruebas");
    public static readonly AmbienteHacienda Produccion = new(1, "Producción");

    private AmbienteHacienda(int id, string name) : base(id, name) { }
}

/// <summary>
/// Modelo de facturación utilizado.
/// </summary>
public sealed class ModeloFacturacion : Enumeration
{
    public static readonly ModeloFacturacion Normal = new(1, "Facturación Normal");
    public static readonly ModeloFacturacion Contingencia = new(2, "Facturación por Contingencia");

    private ModeloFacturacion(int id, string name) : base(id, name) { }
}

/// <summary>
/// Tipo de transmisión del DTE a Hacienda.
/// </summary>
public sealed class TipoTransmision : Enumeration
{
    public static readonly TipoTransmision Normal = new(1, "Normal");
    public static readonly TipoTransmision Contingencia = new(2, "Por Contingencia");

    private TipoTransmision(int id, string name) : base(id, name) { }
}
