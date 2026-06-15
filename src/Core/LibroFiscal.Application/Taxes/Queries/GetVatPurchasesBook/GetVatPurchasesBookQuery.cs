using LibroFiscal.Application.Abstractions.Messaging;
using System.Collections.Generic;

namespace LibroFiscal.Application.Taxes.Queries.GetVatPurchasesBook;

public sealed record GetVatPurchasesBookQuery(int Year, int Month) : IQuery<IReadOnlyList<VatPurchaseDto>>;
