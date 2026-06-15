using System.Linq;
using LibroFiscal.Domain.DTE.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace LibroFiscal.Integrations.Pdf;

public sealed class DteDocumentTemplate : IDocument
{
    private readonly DteDocument _dte;

    public DteDocumentTemplate(DteDocument dte)
    {
        _dte = dte;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;
    public DocumentSettings GetSettings() => DocumentSettings.Default;

    public void Compose(IDocumentContainer container)
    {
        container
            .Page(page =>
            {
                page.Margin(50);
                
                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().Element(ComposeFooter);
            });
    }

    private void ComposeHeader(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text(_dte.Emisor.NombreComercial).FontSize(20).SemiBold().FontColor(Colors.Blue.Darken2);
                column.Item().Text(_dte.Emisor.Nombre).FontSize(12).FontColor(Colors.Grey.Darken2);
                column.Item().Text($"NIT: {_dte.Emisor.Nit}");
                column.Item().Text($"NRC: {_dte.Emisor.Nrc}");
                column.Item().Text($"Actividad: {_dte.Emisor.DescripcionActividad}");
                column.Item().Text($"Dirección: {_dte.Emisor.Direccion.Complemento}, {_dte.Emisor.Direccion.Municipio}, {_dte.Emisor.Direccion.Departamento}");
            });

            row.ConstantItem(250).Background(Colors.Grey.Lighten4).Padding(10).Column(column =>
            {
                column.Item().Text("DOCUMENTO TRIBUTARIO ELECTRÓNICO").FontSize(10).Bold().AlignCenter();
                column.Item().Text("FACTURA").FontSize(14).Bold().AlignCenter().FontColor(Colors.Blue.Darken2);
                column.Item().PaddingTop(5).Text($"Código de Generación:").FontSize(8).Bold();
                column.Item().Text(_dte.CodigoGeneracion).FontSize(8);
                column.Item().PaddingTop(5).Text($"Número de Control:").FontSize(8).Bold();
                column.Item().Text(_dte.NumeroControl.Value).FontSize(8);
                if (!string.IsNullOrEmpty(_dte.SelloRecepcion))
                {
                    column.Item().PaddingTop(5).Text($"Sello de Recepción:").FontSize(8).Bold();
                    column.Item().Text(_dte.SelloRecepcion).FontSize(8);
                }
            });
        });
    }

    private void ComposeContent(IContainer container)
    {
        container.PaddingVertical(20).Column(column =>
        {
            column.Spacing(20);

            column.Item().Element(ComposeClientDetails);
            column.Item().Element(ComposeTable);
            column.Item().Element(ComposeTotals);
        });
    }

    private void ComposeClientDetails(IContainer container)
    {
        container.Background(Colors.Grey.Lighten4).Padding(10).Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text("Cliente").SemiBold();
                if (_dte.Receptor != null)
                {
                    column.Item().Text(_dte.Receptor.Nombre);
                    if (!string.IsNullOrEmpty(_dte.Receptor.Nit))
                    {
                        column.Item().Text($"NIT / DUI: {_dte.Receptor.Nit}");
                    }
                }
                else
                {
                    column.Item().Text("Consumidor Final");
                }
            });

            row.RelativeItem().Column(column =>
            {
                column.Item().Text("Fecha y Hora de Emisión").SemiBold();
                column.Item().Text(_dte.FechaEmision.ToString("dd/MM/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture));
            });
        });
    }

    private void ComposeTable(IContainer container)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(30);  // N
                columns.ConstantColumn(50);  // Cantidad
                columns.RelativeColumn();    // Descripcion
                columns.ConstantColumn(80);  // Precio
                columns.ConstantColumn(80);  // Ventas Gravadas
            });

            table.Header(header =>
            {
                header.Cell().Element(CellStyle).Text("#");
                header.Cell().Element(CellStyle).Text("Cant.");
                header.Cell().Element(CellStyle).Text("Descripción");
                header.Cell().Element(CellStyle).AlignRight().Text("Precio Unit.");
                header.Cell().Element(CellStyle).AlignRight().Text("Ventas Gravadas");

                static IContainer CellStyle(IContainer container)
                {
                    return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                }
            });

            var culture = new System.Globalization.CultureInfo("en-US");
            foreach (var item in _dte.CuerpoDocumento)
            {
                table.Cell().Element(CellStyle).Text(item.NumeroLinea.ToString(culture));
                table.Cell().Element(CellStyle).Text(item.Cantidad.ToString("N2", culture));
                table.Cell().Element(CellStyle).Text(item.Descripcion);
                table.Cell().Element(CellStyle).AlignRight().Text(item.PrecioUnitario.ToString("C", culture));
                table.Cell().Element(CellStyle).AlignRight().Text((item.Cantidad * item.PrecioUnitario).ToString("C", culture));

                static IContainer CellStyle(IContainer container)
                {
                    return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                }
            }
        });
    }

    private void ComposeTotals(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem(); // Spacer
            
            row.ConstantItem(250).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                var culture = new System.Globalization.CultureInfo("en-US");

                table.Cell().Text("Suma de Ventas:").SemiBold();
                table.Cell().AlignRight().Text(_dte.Resumen.SubTotal.ToString("C", culture));

                table.Cell().Text("IVA (13%):").SemiBold();
                table.Cell().AlignRight().Text(_dte.Resumen.TotalIva.ToString("C", culture));
                
                table.Cell().PaddingTop(5).Text("Total a Pagar:").SemiBold().FontSize(14);
                table.Cell().PaddingTop(5).AlignRight().Text(_dte.Resumen.TotalPagar.ToString("C", culture)).SemiBold().FontSize(14).FontColor(Colors.Blue.Darken2);
            });
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.AlignCenter().Text(x =>
        {
            x.Span("Página ");
            x.CurrentPageNumber();
            x.Span(" de ");
            x.TotalPages();
        });
    }
}
