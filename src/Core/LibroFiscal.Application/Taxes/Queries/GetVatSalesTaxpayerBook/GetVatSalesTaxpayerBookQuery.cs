using LibroFiscal.Application.Abstractions.Messaging;
using System.Collections.Generic;

namespace LibroFiscal.Application.Taxes.Queries.GetVatSalesTaxpayerBook;

public sealed record GetVatSalesTaxpayerBookQuery(
    int Year,
    int Month) : IQuery<IReadOnlyList<VatSalesTaxpayerDto>>;
