using LibroFiscal.Application.Abstractions.Messaging;
using System.Collections.Generic;

namespace LibroFiscal.Application.Taxes.Queries.GetVatSalesConsumerBook;

public sealed record GetVatSalesConsumerBookQuery(
    System.Guid CompanyId,
    int Year,
    int Month) : IQuery<IReadOnlyList<VatSalesConsumerDto>>;
