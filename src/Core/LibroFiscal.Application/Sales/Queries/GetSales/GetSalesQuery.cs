using LibroFiscal.Domain.Common.Ids;
using LibroFiscal.Domain.Sales.Entities;
using LibroFiscal.SharedKernel.Interfaces;
using LibroFiscal.SharedKernel.Results;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LibroFiscal.Application.Sales.Queries.GetSales;

public sealed record GetSalesQuery(Guid CompanyId) : IRequest<Result<IReadOnlyList<SaleDto>>>;

public sealed record SaleDto(
    Guid Id,
    string CustomerName,
    string CustomerNit,
    string DocumentNumber,
    DateTimeOffset IssueDate,
    decimal TotalAmount);

internal sealed class GetSalesQueryHandler : IRequestHandler<GetSalesQuery, Result<IReadOnlyList<SaleDto>>>
{
    private readonly IRepository<Sale, SaleId> _saleRepository;

    public GetSalesQueryHandler(IRepository<Sale, SaleId> saleRepository)
    {
        _saleRepository = saleRepository;
    }

    public async Task<Result<IReadOnlyList<SaleDto>>> Handle(GetSalesQuery request, CancellationToken cancellationToken)
    {
        var companyId = CompanyId.From(request.CompanyId);

        var sales = await _saleRepository.FindAsync(
            s => s.CompanyId == companyId,
            q => q.OrderByDescending(s => s.IssueDate),
            cancellationToken);

        var result = sales.Select(s => new SaleDto(
            s.Id.Value,
            s.CustomerName,
            s.CustomerNit,
            s.DocumentNumber,
            s.IssueDate,
            s.TotalAmount)).ToList();

        return result;
    }
}
