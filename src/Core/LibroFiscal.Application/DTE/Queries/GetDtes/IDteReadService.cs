using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LibroFiscal.Application.DTE.Queries.GetDtes;

public interface IDteReadService
{
    Task<List<DteSummaryDto>> GetDtesAsync(CancellationToken cancellationToken = default);
}
