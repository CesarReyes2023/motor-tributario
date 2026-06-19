using LibroFiscal.Application.Abstractions.Messaging;
using LibroFiscal.Domain.Common.Ids;
using LibroFiscal.Domain.Purchases.Entities;
using LibroFiscal.SharedKernel.Interfaces;
using LibroFiscal.SharedKernel.Results;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LibroFiscal.Application.Taxes.Queries.GetVatPurchasesBook;

internal sealed class GetVatPurchasesBookQueryHandler : IQueryHandler<GetVatPurchasesBookQuery, IReadOnlyList<VatPurchaseDto>>
{
    private readonly IRepository<Purchase, PurchaseId> _purchaseRepository;

    public GetVatPurchasesBookQueryHandler(IRepository<Purchase, PurchaseId> purchaseRepository)
    {
        _purchaseRepository = purchaseRepository;
    }

    public async Task<Result<IReadOnlyList<VatPurchaseDto>>> Handle(GetVatPurchasesBookQuery request, CancellationToken cancellationToken)
    {
        // Obtener compras del mes y año especificado
        var startDate = new System.DateTimeOffset(new System.DateTime(request.Year, request.Month, 1), System.TimeSpan.Zero);
        var endDate = startDate.AddMonths(1);

        var companyId = new CompanyId(request.CompanyId);
        var purchases = await _purchaseRepository.FindAsync(
            p => p.CompanyId == companyId && p.IssueDate >= startDate && p.IssueDate < endDate, 
            q => q.OrderBy(p => p.IssueDate).ThenBy(p => p.DocumentNumber),
            cancellationToken);

        var dtos = new List<VatPurchaseDto>();
        int rowNumber = 1;

        foreach (var p in purchases)
        {
            // Asumimos para este MVP que todas las compras con IVA van a "InternalPurchases" y su IVA a "TaxCredit"
            // Las compras sin IVA van a "ExemptPurchases"
            
            decimal exempt = p.TaxAmount == 0 ? p.SubTotal : 0;
            decimal internalPurchases = p.TaxAmount > 0 ? p.SubTotal : 0;
            decimal taxCredit = p.TaxAmount;
            
            dtos.Add(new VatPurchaseDto(
                p.Id.Value,
                rowNumber++,
                p.IssueDate,
                p.DocumentNumber,
                p.SupplierNit,
                "000000-0", // Proveedor NRC (mock, faltaría añadir a entidad si se requiere)
                p.SupplierName,
                exempt,
                internalPurchases,
                0, // Imports
                taxCredit,
                0, // RetainedTax
                0, // Excluded
                p.TotalAmount
            ));
        }

        return dtos;
    }
}
