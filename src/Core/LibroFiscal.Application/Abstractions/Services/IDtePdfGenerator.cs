using System;
using System.Threading;
using System.Threading.Tasks;
using LibroFiscal.Domain.DTE.Entities;
using LibroFiscal.SharedKernel.Results;

namespace LibroFiscal.Application.Abstractions.Services;

public interface IDtePdfGenerator
{
    /// <summary>
    /// Generates a PDF byte array for the given DTE document.
    /// </summary>
    Task<Result<byte[]>> GeneratePdfAsync(DteDocument dte, CancellationToken cancellationToken = default);
}
