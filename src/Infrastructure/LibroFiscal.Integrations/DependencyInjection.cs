using LibroFiscal.Application.Abstractions.Services;
using LibroFiscal.Integrations.Hacienda;
using LibroFiscal.Integrations.Signing;
using Microsoft.Extensions.DependencyInjection;

namespace LibroFiscal.Integrations;

public static class DependencyInjection
{
    public static IServiceCollection AddIntegrations(this IServiceCollection services)
    {
        services.AddHttpClient<IHaciendaService, HaciendaApiClient>();
        // Registrar Parsers de Ingesta Masiva
        services.AddScoped<LibroFiscal.Application.DteIngestion.Services.IDteParserService, Hacienda.Ingestion.HaciendaJsonParserService>();
        services.AddTransient<IDtePdfGenerator, LibroFiscal.Integrations.Pdf.QuestPdfGeneratorService>();
        services.AddTransient<IHtmlToPdfGenerator, LibroFiscal.Integrations.Pdf.HtmlToPdfGenerator>();
        services.AddTransient<LibroFiscal.Application.OCR.Services.IOcrScannerService, LibroFiscal.Integrations.Ocr.TesseractOcrScannerService>();
        services.AddTransient<IHaciendaF930ExportService, Hacienda.Exports.HaciendaF930ExportService>();
        services.AddTransient<ISigningService, FirmadorService>();

        return services;
    }
}
