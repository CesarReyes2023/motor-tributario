using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LibroFiscal.Application.Abstractions.Messaging;
using LibroFiscal.SharedKernel.Interfaces;
using LibroFiscal.Domain.Common.Ids;
using LibroFiscal.Domain.DTE.Entities;
using LibroFiscal.SharedKernel.Results;

namespace LibroFiscal.Application.DTE.Queries.GetDtes;

public record GetDtesQuery() : IQuery<List<DteSummaryDto>>;

public sealed class GetDtesQueryHandler : IQueryHandler<GetDtesQuery, List<DteSummaryDto>>
{
    private readonly IDteReadService _readService;

    public GetDtesQueryHandler(IDteReadService readService)
    {
        _readService = readService;
    }

    public async Task<Result<List<DteSummaryDto>>> Handle(GetDtesQuery request, CancellationToken cancellationToken)
    {
        var dtos = await _readService.GetDtesAsync(cancellationToken);
        return Result.Success(dtos);
    }
}
