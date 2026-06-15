using LibroFiscal.Application.Abstractions.Messaging;
using LibroFiscal.Application.OCR.DTOs;

namespace LibroFiscal.Application.OCR.Commands.ScanDte;

public sealed record ScanDteCommand(byte[] ImageBytes) : ICommand<OcrResultDto>;
