namespace LibroFiscal.Application.DteIngestion.Catalogs;

/// <summary>
/// Catálogos Oficiales del Ministerio de Hacienda de El Salvador (Facturación Electrónica V1.0)
/// </summary>
public static class HaciendaCatalogs
{
    public static class TipoDocumento
    {
        public const string Factura = "01";
        public const string ComprobanteCreditoFiscal = "03";
        public const string NotaRemision = "04";
        public const string NotaCredito = "05";
        public const string NotaDebito = "06";
        public const string ComprobanteRetencion = "07";
        public const string FacturaExportacion = "11";
        public const string FacturaSujetoExcluido = "14";
    }

    public static class Tributos
    {
        public const string Iva = "20"; // Impuesto al Valor Agregado 13%
        public const string IvaExportaciones = "C3"; // 0%
    }

    public static class TipoDocumentoIdentificacion
    {
        public const string Nit = "36";
        public const string Dui = "13";
        public const string Pasaporte = "03";
        public const string CarnetResidente = "02";
        public const string Otro = "37";
    }

    public static class CondicionOperacion
    {
        public const string Contado = "1";
        public const string Credito = "2";
        public const string Otro = "3";
    }
}
