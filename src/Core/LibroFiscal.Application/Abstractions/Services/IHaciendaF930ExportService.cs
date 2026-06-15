using LibroFiscal.Application.Taxes.Queries.GetVatPurchasesBook;
using LibroFiscal.Application.Taxes.Queries.GetVatSalesConsumerBook;
using LibroFiscal.Application.Taxes.Queries.GetVatSalesTaxpayerBook;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LibroFiscal.Application.Abstractions.Services;

public interface IHaciendaF930ExportService
{
    Task ExportPurchasesAsync(string filePath, IEnumerable<VatPurchaseDto> purchases);
    Task ExportSalesTaxpayerAsync(string filePath, IEnumerable<VatSalesTaxpayerDto> sales);
    Task ExportSalesConsumerAsync(string filePath, IEnumerable<VatSalesConsumerDto> sales);
}
