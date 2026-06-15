using LibroFiscal.Application.Abstractions.Services;
using LibroFiscal.Application.Taxes.Queries.GetVatPurchasesBook;
using LibroFiscal.Application.Taxes.Queries.GetVatSalesConsumerBook;
using LibroFiscal.Application.Taxes.Queries.GetVatSalesTaxpayerBook;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LibroFiscal.Integrations.Hacienda.Exports;

public sealed class HaciendaF930ExportService : IHaciendaF930ExportService
{
    public async Task ExportPurchasesAsync(string filePath, IEnumerable<VatPurchaseDto> purchases)
    {
        var sb = new StringBuilder();

        foreach (var p in purchases)
        {
            // Formato F930 Anexo Compras
            // 1. Fecha de Emision (DD/MM/YYYY)
            // 2. Clase Documento (03 = CCF)
            // 3. Tipo Documento (1 = Impreso, 2 = Electronico) Asumiremos 1 por ahora o 2 dependiendo del Control
            // 4. Numero Documento
            // 5. NIT Proveedor
            // 6. NRC Proveedor
            // 7. Nombre Proveedor
            // 8. Compras Exentas Internas
            // 9. Compras Exentas Importaciones
            // 10. Compras Gravadas Internas
            // 11. Compras Gravadas Importaciones
            // 12. Credito Fiscal
            // 13. IVA Retenido
            // 14. Impuesto FOVIAL (0.00)
            // 15. Impuesto COTRANS (0.00)

            var claseDoc = "03"; // Comprobante de Credito Fiscal
            var tipoDoc = "1"; // 1=Físico, 2=Electrónico
            var nit = p.SupplierNit?.Replace("-", "") ?? "";
            var nrc = p.SupplierNrc?.Replace("-", "") ?? "";

            var line = string.Format(CultureInfo.InvariantCulture,
                "{0:dd/MM/yyyy};{1};{2};{3};{4};{5};{6};{7:F2};0.00;{8:F2};{9:F2};{10:F2};{11:F2};0.00;0.00",
                p.IssueDate, claseDoc, tipoDoc, p.DocumentNumber, nit, nrc, p.SupplierName,
                p.ExemptPurchases, p.InternalPurchases, p.Imports, p.TaxCredit, p.RetainedTax);

            sb.AppendLine(line);
        }

        await File.WriteAllTextAsync(filePath, sb.ToString(), Encoding.UTF8);
    }

    public async Task ExportSalesTaxpayerAsync(string filePath, IEnumerable<VatSalesTaxpayerDto> sales)
    {
        var sb = new StringBuilder();

        foreach (var s in sales)
        {
            // Formato F930 Anexo Ventas Contribuyentes
            var claseDoc = "03"; 
            var tipoDoc = "1"; 
            var nit = ""; // DTO current doesn't have CustomerNit
            var nrc = s.NrcCustomer?.Replace("-", "") ?? "";

            var line = string.Format(CultureInfo.InvariantCulture,
                "{0:dd/MM/yyyy};{1};{2};{3};{4};{5};{6};{7:F2};{8:F2};{9:F2};{10:F2};0.00;0.00",
                s.EmisionDate, claseDoc, tipoDoc, s.DocumentNumber, nit, nrc, s.CustomerName,
                s.ExemptSales, s.LocalGravadaSales, s.FiscalDebit, s.RetainedIva);

            sb.AppendLine(line);
        }

        await File.WriteAllTextAsync(filePath, sb.ToString(), Encoding.UTF8);
    }

    public async Task ExportSalesConsumerAsync(string filePath, IEnumerable<VatSalesConsumerDto> sales)
    {
        var sb = new StringBuilder();

        foreach (var s in sales)
        {
            // Formato F930 Anexo Ventas Consumidor Final
            var claseDoc = "01"; // Factura
            var tipoDoc = "1"; 

            var line = string.Format(CultureInfo.InvariantCulture,
                "{0:dd/MM/yyyy};{1};{2};{3};{4};{5:F2};{6:F2};{7:F2};{8:F2}",
                s.EmisionDate, claseDoc, tipoDoc, s.InitialDocumentNumber, s.FinalDocumentNumber,
                s.ExemptSales, s.LocalGravadaSales, s.ExportSales, s.RetainedIva);

            sb.AppendLine(line);
        }

        await File.WriteAllTextAsync(filePath, sb.ToString(), Encoding.UTF8);
    }
}
