using LibroFiscal.Application.DteIngestion.Catalogs;
using LibroFiscal.Domain.Taxes.Enums;
using System;

namespace LibroFiscal.Application.DteIngestion.Services;

/// <summary>
/// Capa Anticorrupción (ACL) para traducir códigos numéricos de Hacienda a enums y conceptos del Dominio.
/// </summary>
public static class HaciendaCatalogTranslator
{
    /// <summary>
    /// Traduce CAT-002 (Tipo Documento) a nuestro DocumentType.
    /// </summary>
    public static string TranslateDocumentType(string codigoHacienda)
    {
        return codigoHacienda switch
        {
            HaciendaCatalogs.TipoDocumento.Factura => "Factura",
            HaciendaCatalogs.TipoDocumento.ComprobanteCreditoFiscal => "Crédito Fiscal",
            HaciendaCatalogs.TipoDocumento.FacturaExportacion => "Exportación",
            HaciendaCatalogs.TipoDocumento.FacturaSujetoExcluido => "Sujeto Excluido",
            _ => throw new ArgumentException($"Código de Tipo de Documento no soportado: {codigoHacienda}")
        };
    }

    /// <summary>
    /// Determina si un Tributo (CAT-015) representa IVA.
    /// </summary>
    public static bool IsIvaTax(string codigoTributo)
    {
        return codigoTributo == HaciendaCatalogs.Tributos.Iva;
    }
}
