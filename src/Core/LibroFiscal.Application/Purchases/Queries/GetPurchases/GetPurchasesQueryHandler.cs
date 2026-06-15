using LibroFiscal.Application.Abstractions.Messaging;
using LibroFiscal.Domain.Common.Ids;
using LibroFiscal.Domain.Purchases.Entities;
using LibroFiscal.SharedKernel.Interfaces;
using LibroFiscal.SharedKernel.Results;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LibroFiscal.Application.Purchases.Queries.GetPurchases;

internal sealed class GetPurchasesQueryHandler : IQueryHandler<GetPurchasesQuery, IReadOnlyList<PurchaseDto>>
{
    private readonly IRepository<Purchase, PurchaseId> _purchaseRepository;

    public GetPurchasesQueryHandler(IRepository<Purchase, PurchaseId> purchaseRepository)
    {
        _purchaseRepository = purchaseRepository;
    }

    public async Task<Result<IReadOnlyList<PurchaseDto>>> Handle(GetPurchasesQuery request, CancellationToken cancellationToken)
    {
        var purchases = await _purchaseRepository.FindAsync(p => true, cancellationToken);

        var dtos = purchases
            .OrderByDescending(p => p.IssueDate)
            .Select(p => new PurchaseDto(
                p.Id.Value,
                p.SupplierNit,
                p.SupplierName,
                p.IssueDate,
                p.DocumentNumber,
                p.SubTotal,
                p.TaxAmount,
                p.TotalAmount,
                p.JournalEntryId?.Value))
            .ToList();

        return dtos;
    }
}
