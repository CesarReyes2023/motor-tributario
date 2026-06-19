using LibroFiscal.Application.Abstractions.Messaging;
using System.Collections.Generic;

namespace LibroFiscal.Application.Taxes.Queries.GetVatPurchasesBook;

public sealed record GetVatPurchasesBookQuery(System.Guid CompanyId, int Year, int Month) : IQuery<IReadOnlyList<VatPurchaseDto>>;
