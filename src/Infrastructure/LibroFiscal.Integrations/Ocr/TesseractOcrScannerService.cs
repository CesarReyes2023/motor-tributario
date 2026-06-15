using LibroFiscal.Application.OCR.DTOs;
using LibroFiscal.Application.OCR.Services;
using LibroFiscal.SharedKernel.Results;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Tesseract;
using UglyToad.PdfPig;

namespace LibroFiscal.Integrations.Ocr;

public sealed class TesseractOcrScannerService : IOcrScannerService
{
    private readonly string _tessDataPath;

    public TesseractOcrScannerService()
    {
        _tessDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata");
        if (!Directory.Exists(_tessDataPath))
        {
            _tessDataPath = Path.Combine(Directory.GetCurrentDirectory(), "tessdata");
        }
    }

    public async Task<Result<OcrResultDto>> ScanImageAsync(byte[] imageBytes, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            try
            {
                if (!Directory.Exists(_tessDataPath))
                {
                    return Result.Failure<OcrResultDto>(Error.Failure("OCR.MissingData", $"No se encontró la carpeta tessdata en {_tessDataPath}."));
                }

                string extractedText = string.Empty;

                // Check if file is PDF (magic number %PDF)
                if (IsPdf(imageBytes))
                {
                    extractedText = ProcessPdf(imageBytes);
                }
                else
                {
                    // Es una imagen directa
                    extractedText = ProcessImageWithTesseract(imageBytes);
                }

                var dto = ParseExtractedText(extractedText);
                return Result.Success(dto);
            }
            catch (Exception ex)
            {
                return Result.Failure<OcrResultDto>(Error.Failure("OCR.ProcessError", $"Error al procesar el archivo: {ex.Message}"));
            }
        }, cancellationToken);
    }

    private static bool IsPdf(byte[] bytes)
    {
        if (bytes.Length < 4) return false;
        // %PDF -> 37 80 68 70 (Hex: 25 50 44 46)
        return bytes[0] == 0x25 && bytes[1] == 0x50 && bytes[2] == 0x44 && bytes[3] == 0x46;
    }

    private string ProcessPdf(byte[] pdfBytes)
    {
        var sb = new StringBuilder();
        
        using (var document = PdfDocument.Open(pdfBytes))
        {
            foreach (var page in document.GetPages())
            {
                var text = page.Text;
                if (!string.IsNullOrWhiteSpace(text) && text.Length > 50)
                {
                    // Digital PDF - Text extracted successfully
                    sb.AppendLine(text);
                }
                else
                {
                    // Scanned PDF - No text layer, try extracting embedded images
                    foreach (var image in page.GetImages())
                    {
                        if (image.TryGetPng(out var pngBytes))
                        {
                            var imgText = ProcessImageWithTesseract(pngBytes);
                            sb.AppendLine(imgText);
                        }
                    }
                }
            }
        }
        
        return sb.ToString();
    }

    private string ProcessImageWithTesseract(byte[] imageBytes)
    {
        var tempPath = Path.GetTempFileName();
        File.WriteAllBytes(tempPath, imageBytes);

        string extractedText;
        using (var engine = new TesseractEngine(_tessDataPath, "spa", EngineMode.Default))
        {
            using (var img = Pix.LoadFromFile(tempPath))
            {
                using (var page = engine.Process(img))
                {
                    extractedText = page.GetText();
                }
            }
        }

        File.Delete(tempPath);
        return extractedText;
    }

    private static OcrResultDto ParseExtractedText(string text)
    {
        string? nit = null;
        string? nrc = null;
        decimal? total = null;
        decimal? iva = null;
        DateTimeOffset? fecha = null;

        // Limpiar el texto un poco para evitar problemas de dobles espacios
        var normalizedText = Regex.Replace(text, @"\s+", " ");

        // NIT format: 0614-230988-102-1 or 06142309881021
        var nitMatch = Regex.Match(normalizedText, @"(?i)NIT\s*[:\-]?\s*(\d{4}-?\d{6}-?\d{3}-?\d{1})");
        if (nitMatch.Success) nit = nitMatch.Groups[1].Value;
        else 
        {
            // Fallback: search for exactly 14 digits with or without dashes
            var fallbackNit = Regex.Match(normalizedText, @"\b(\d{4}-?\d{6}-?\d{3}-?\d{1})\b");
            if (fallbackNit.Success) nit = fallbackNit.Groups[1].Value;
        }

        // NRC format: 123456-7 or 1234567
        var nrcMatch = Regex.Match(normalizedText, @"(?i)NRC\s*[:\-]?\s*([0-9\-]{6,9})");
        if (nrcMatch.Success) nrc = nrcMatch.Groups[1].Value.Trim();

        // Date format: 14/06/2026 or 14-06-2026
        var dateMatch = Regex.Match(normalizedText, @"\b(\d{2}[/-]\d{2}[/-]\d{4})\b");
        if (dateMatch.Success)
        {
            var dateStr = dateMatch.Groups[1].Value.Replace("-", "/");
            if (DateTimeOffset.TryParseExact(dateStr, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeLocal, out var parsedDate))
            {
                fecha = parsedDate;
            }
            else if (DateTimeOffset.TryParse(dateStr, out var fallbackDate))
            {
                fecha = fallbackDate;
            }
        }

        // Try to find something that looks like money (Total)
        var totalMatch = Regex.Match(normalizedText, @"(?i)(total(?: a pagar)?|suma de ventas|monto)[\s:\.\$]*([\d,\.]+)");
        if (totalMatch.Success)
        {
            var numberStr = totalMatch.Groups[2].Value.Replace(",", "");
            if (decimal.TryParse(numberStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var parsedTotal))
            {
                total = parsedTotal;
            }
        }

        // Try to find IVA
        var ivaMatch = Regex.Match(normalizedText, @"(?i)(iva)[\s:\.\$]*([\d,\.]+)");
        if (ivaMatch.Success)
        {
            var numberStr = ivaMatch.Groups[2].Value.Replace(",", "");
            if (decimal.TryParse(numberStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var parsedIva))
            {
                iva = parsedIva;
            }
        }

        return new OcrResultDto(text, nit, nrc, total, iva, fecha);
    }
}
