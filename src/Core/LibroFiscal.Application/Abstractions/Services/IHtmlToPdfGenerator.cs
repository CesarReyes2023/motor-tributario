using System.Threading;
using System.Threading.Tasks;

namespace LibroFiscal.Application.Abstractions.Services;

public interface IHtmlToPdfGenerator
{
    /// <summary>
    /// Converts a raw HTML string into a PDF byte array.
    /// </summary>
    Task<byte[]> GeneratePdfAsync(string htmlContent, CancellationToken cancellationToken = default);
}
