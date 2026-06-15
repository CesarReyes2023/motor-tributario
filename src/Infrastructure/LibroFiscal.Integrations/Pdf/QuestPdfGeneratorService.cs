using System.IO;
using System.Threading;
using System.Threading.Tasks;
using LibroFiscal.Application.Abstractions.Services;
using LibroFiscal.Domain.DTE.Entities;
using LibroFiscal.SharedKernel.Results;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace LibroFiscal.Integrations.Pdf;

public sealed class QuestPdfGeneratorService : IDtePdfGenerator
{
    public QuestPdfGeneratorService()
    {
        // En una aplicación comercial esto requiere licencia,
        // para este proyecto MVP se utiliza la licencia Community.
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public Task<Result<byte[]>> GeneratePdfAsync(DteDocument dte, CancellationToken cancellationToken = default)
    {
        try
        {
            var document = new DteDocumentTemplate(dte);
            var bytes = document.GeneratePdf();

            return Task.FromResult(Result.Success(bytes));
        }
        catch (System.Exception ex)
        {
            return Task.FromResult(Result.Failure<byte[]>(Error.Failure("Pdf.GenerationError", $"Error al generar el PDF: {ex.Message}")));
        }
    }
}
