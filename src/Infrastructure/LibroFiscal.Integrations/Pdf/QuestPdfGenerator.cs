using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LibroFiscal.Application.Abstractions.Services;
using LibroFiscal.Domain.DTE.Entities;
using LibroFiscal.SharedKernel.Results;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace LibroFiscal.Integrations.Pdf;

public sealed class QuestPdfGenerator : IDtePdfGenerator
{
    public QuestPdfGenerator()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public Task<Result<byte[]>> GeneratePdfAsync(DteDocument dte, CancellationToken cancellationToken = default)
    {
        try
        {
            var pdfData = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));

                    page.Header().Element(header => ComposeHeader(header, dte));
                    page.Content().Element(content => ComposeContent(content, dte));
                    page.Footer().Element(footer => ComposeFooter(footer, dte));
                });
            })
            .GeneratePdf();

            return Task.FromResult(Result.Success(pdfData));
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result.Failure<byte[]>(Error.Failure("Pdf.GenerationFailed", $"Error generando PDF: {ex.Message}")));
        }
    }

    private static void ComposeHeader(IContainer container, DteDocument dte)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text(dte.Emisor.NombreComercial ?? dte.Emisor.Nombre).FontSize(20).SemiBold().FontColor(Colors.Blue.Darken2);
                column.Item().Text($"NIT: {dte.Emisor.Nit}");
                column.Item().Text($"NRC: {dte.Emisor.Nrc}");
                column.Item().Text($"Actividad: {dte.Emisor.DescripcionActividad}");
                column.Item().Text($"Dirección: {dte.Emisor.Direccion.Complemento}, {dte.Emisor.Direccion.Municipio}");
                column.Item().Text($"Tel: {dte.Emisor.Telefono} | Email: {dte.Emisor.Correo}");
            });

            row.ConstantItem(180).Column(column =>
            {
                column.Item().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(5).Column(c => 
                {
                    c.Item().Text("DOCUMENTO TRIBUTARIO ELECTRÓNICO").FontSize(10).Bold().AlignCenter();
                    c.Item().Text(dte.TipoDte.Name).FontSize(12).Bold().FontColor(Colors.Red.Medium).AlignCenter();
                    c.Item().PaddingTop(5).Text($"Código: {dte.CodigoGeneracion}").FontSize(8).AlignCenter();
                    c.Item().Text($"Control: {dte.NumeroControl?.Value}").FontSize(8).AlignCenter();
                    c.Item().Text($"Sello: {dte.SelloRecepcion ?? "SIN SELLO"}").FontSize(8).AlignCenter();
                    c.Item().PaddingTop(5).Text($"Fecha: {dte.FechaEmision:dd/MM/yyyy HH:mm}").FontSize(9).AlignCenter();
                });
            });
        });
    }

    private static void ComposeContent(IContainer container, DteDocument dte)
    {
        container.PaddingVertical(1, Unit.Centimetre).Column(column =>
        {
            column.Spacing(5);

            // Receptor Info
            column.Item().Background(Colors.Grey.Lighten3).Padding(5).Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("CLIENTE").SemiBold();
                    c.Item().Text(dte.Receptor?.Nombre ?? "Consumidor Final");
                    if (!string.IsNullOrEmpty(dte.Receptor?.NumeroDocumento))
                        c.Item().Text($"Doc: {dte.Receptor.NumeroDocumento}");
                });
                row.RelativeItem().Column(c =>
                {
                    if (!string.IsNullOrEmpty(dte.Receptor?.Nit))
                        c.Item().Text($"NIT: {dte.Receptor.Nit}");
                    if (!string.IsNullOrEmpty(dte.Receptor?.Nrc))
                        c.Item().Text($"NRC: {dte.Receptor.Nrc}");
                });
            });

            column.Item().PaddingTop(10).Element(table => ComposeTable(table, dte));

            var totalPagar = dte.Resumen?.TotalPagar ?? 0m;
            column.Item().PaddingTop(10).AlignRight().Text($"Total a Pagar: ${totalPagar.ToString("N2", System.Globalization.CultureInfo.InvariantCulture)}").FontSize(14).SemiBold();
        });
    }

    private static void ComposeTable(IContainer container, DteDocument dte)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(40);
                columns.RelativeColumn();
                columns.ConstantColumn(60);
                columns.ConstantColumn(80);
                columns.ConstantColumn(80);
            });

            table.Header(header =>
            {
                header.Cell().Element(CellStyle).Text("#");
                header.Cell().Element(CellStyle).Text("Descripción");
                header.Cell().Element(CellStyle).AlignRight().Text("Cant.");
                header.Cell().Element(CellStyle).AlignRight().Text("Precio Unit.");
                header.Cell().Element(CellStyle).AlignRight().Text("Subtotal");

                static IContainer CellStyle(IContainer container)
                {
                    return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                }
            });

            if (dte.CuerpoDocumento != null)
            {
                foreach (var item in dte.CuerpoDocumento)
                {
                    table.Cell().Element(CellStyle).Text(item.NumeroLinea.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    table.Cell().Element(CellStyle).Text(item.Descripcion);
                    table.Cell().Element(CellStyle).AlignRight().Text(item.Cantidad.ToString("N2", System.Globalization.CultureInfo.InvariantCulture));
                    table.Cell().Element(CellStyle).AlignRight().Text($"${item.PrecioUnitario.ToString("N2", System.Globalization.CultureInfo.InvariantCulture)}");
                    table.Cell().Element(CellStyle).AlignRight().Text($"${(item.Cantidad * item.PrecioUnitario).ToString("N2", System.Globalization.CultureInfo.InvariantCulture)}");

                    static IContainer CellStyle(IContainer container)
                    {
                        return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                    }
                }
            }
        });
    }

    private static void ComposeFooter(IContainer container, DteDocument dte)
    {
        container.AlignCenter().Text(x =>
        {
            x.Span("Generado con LibroFiscal - ");
            x.Span("Página ");
            x.CurrentPageNumber();
            x.Span(" de ");
            x.TotalPages();
        });
    }
}
