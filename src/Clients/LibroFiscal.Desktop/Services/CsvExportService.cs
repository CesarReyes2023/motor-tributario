using LibroFiscal.Application.Taxes.Queries.GetVatPurchasesBook;
using LibroFiscal.Application.Taxes.Queries.GetVatSalesConsumerBook;
using LibroFiscal.Application.Taxes.Queries.GetVatSalesTaxpayerBook;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LibroFiscal.Desktop.Services;

[SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Injected via DI")]
public sealed class CsvExportService
{
    public async Task ExportPurchasesAsync(string filePath, IEnumerable<VatPurchaseDto> purchases)
    {
        var sb = new StringBuilder();
        // Encabezados (Aproximación Anexo F-07 El Salvador)
        sb.AppendLine("Correlativo;Fecha Emision;Numero Documento;NIT Proveedor;NRC Proveedor;Nombre Proveedor;Compras Exentas;Compras Gravadas;Importaciones;Credito Fiscal;IVA Retenido;Total");

        foreach (var p in purchases)
        {
            var line = string.Format(CultureInfo.InvariantCulture, "{0};{1:dd/MM/yyyy};{2};{3};{4};{5};{6:F2};{7:F2};{8:F2};{9:F2};{10:F2};{11:F2}",
                p.RowNumber, p.IssueDate, p.DocumentNumber, p.SupplierNit, p.SupplierNrc, p.SupplierName, 
                p.ExemptPurchases, p.InternalPurchases, p.Imports, p.TaxCredit, p.RetainedTax, p.TotalPurchases);
            sb.AppendLine(line);
        }

        await File.WriteAllTextAsync(filePath, sb.ToString(), Encoding.UTF8);
    }

    public async Task ExportSalesTaxpayerAsync(string filePath, IEnumerable<VatSalesTaxpayerDto> sales)
    {
        var sb = new StringBuilder();
        // Encabezados
        sb.AppendLine("Correlativo;Fecha Emision;Numero CCF;NIT Cliente;NRC Cliente;Nombre Cliente;Ventas Exentas;Ventas Gravadas Locales;Debito Fiscal;IVA Retenido;Total Operacion");

        int i = 1;
        foreach (var s in sales)
        {
            var line = string.Format(CultureInfo.InvariantCulture, "{0};{1:dd/MM/yyyy};{2};;{3};{4};{5:F2};{6:F2};{7:F2};{8:F2};{9:F2}",
                i++, s.EmisionDate, s.DocumentNumber, s.NrcCustomer, s.CustomerName, 
                s.ExemptSales, s.LocalGravadaSales, s.FiscalDebit, s.RetainedIva, s.TotalSales);
            sb.AppendLine(line);
        }

        await File.WriteAllTextAsync(filePath, sb.ToString(), Encoding.UTF8);
    }

    public async Task ExportSalesConsumerAsync(string filePath, IEnumerable<VatSalesConsumerDto> sales)
    {
        var sb = new StringBuilder();
        // Encabezados
        sb.AppendLine("Correlativo;Fecha Emision;Del Numero;Al Numero;Ventas Exentas;Ventas Gravadas Locales;Exportaciones;Total Venta Diaria;IVA Retenido");

        int i = 1;
        foreach (var s in sales)
        {
            var line = string.Format(CultureInfo.InvariantCulture, "{0};{1:dd/MM/yyyy};{2};{3};{4:F2};{5:F2};{6:F2};{7:F2};{8:F2}",
                i++, s.EmisionDate, s.InitialDocumentNumber, s.FinalDocumentNumber, 
                s.ExemptSales, s.LocalGravadaSales, s.ExportSales, s.TotalSales, s.RetainedIva);
            sb.AppendLine(line);
        }

        await File.WriteAllTextAsync(filePath, sb.ToString(), Encoding.UTF8);
    }
}
