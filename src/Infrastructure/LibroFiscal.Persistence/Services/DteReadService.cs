using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LibroFiscal.Application.DTE.Queries.GetDtes;
using Microsoft.EntityFrameworkCore;

namespace LibroFiscal.Persistence.Services;

public sealed class DteReadService : IDteReadService
{
    private readonly LibroFiscalDbContext _dbContext;

    public DteReadService(LibroFiscalDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<DteSummaryDto>> GetDtesAsync(System.Guid companyId, CancellationToken cancellationToken = default)
    {
        var dtes = await _dbContext.Dtes
            .AsNoTracking()
            .Where(d => d.CompanyId == LibroFiscal.Domain.Common.Ids.CompanyId.From(companyId))
            .OrderByDescending(d => d.FechaEmision)
            .Take(100)
            .Select(d => new DteSummaryDto(
                d.Id.Value,
                d.NumeroControl.Value,
                "Factura",
                d.Receptor == null ? "Consumidor Final" : d.Receptor.Nombre,
                d.Resumen.TotalPagar,
                d.Estado.Name,
                d.SelloRecepcion,
                d.FechaEmision
            ))
            .ToListAsync(cancellationToken);

        return dtes;
    }
}
