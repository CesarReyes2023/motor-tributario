using LibroFiscal.Application.Abstractions.Messaging;
using LibroFiscal.Application.OCR.DTOs;
using LibroFiscal.Application.OCR.Services;
using LibroFiscal.SharedKernel.Results;
using System.Threading;
using System.Threading.Tasks;

namespace LibroFiscal.Application.OCR.Commands.ScanDte;

public sealed class ScanDteCommandHandler : ICommandHandler<ScanDteCommand, OcrResultDto>
{
    private readonly IOcrScannerService _ocrScannerService;

    public ScanDteCommandHandler(IOcrScannerService ocrScannerService)
    {
        _ocrScannerService = ocrScannerService;
    }

    public async Task<Result<OcrResultDto>> Handle(ScanDteCommand request, CancellationToken cancellationToken)
    {
        if (request.ImageBytes == null || request.ImageBytes.Length == 0)
        {
            return Result.Failure<OcrResultDto>(Error.Validation("OCR.EmptyImage", "La imagen proporcionada está vacía."));
        }

        return await _ocrScannerService.ScanImageAsync(request.ImageBytes, cancellationToken);
    }
}
