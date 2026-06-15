using LibroFiscal.Application.OCR.DTOs;
using LibroFiscal.SharedKernel.Results;
using System.Threading;
using System.Threading.Tasks;

namespace LibroFiscal.Application.OCR.Services;

public interface IOcrScannerService
{
    Task<Result<OcrResultDto>> ScanImageAsync(byte[] imageBytes, CancellationToken cancellationToken = default);
}
