using LibroFiscal.Application.Abstractions.Messaging;
using System.Collections.Generic;

namespace LibroFiscal.Application.Purchases.Queries.GetPurchases;

public sealed record GetPurchasesQuery(System.Guid CompanyId) : IQuery<IReadOnlyList<PurchaseDto>>;
