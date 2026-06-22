using LibroFiscal.Application.Abstractions.Services;
using LibroFiscal.Domain.Common.Ids;
using LibroFiscal.Domain.Companies.Entities;
using LibroFiscal.Domain.Sales.Entities;
using LibroFiscal.SharedKernel.Interfaces;
using LibroFiscal.SharedKernel.Results;
using MediatR;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LibroFiscal.Application.Invoices.Commands.GeneratePdf;

public sealed record GenerateInvoicePdfCommand(Guid SaleId, Guid InvoiceTemplateId) : IRequest<Result<byte[]>>;

internal sealed class GenerateInvoicePdfCommandHandler : IRequestHandler<GenerateInvoicePdfCommand, Result<byte[]>>
{
    private readonly IHtmlToPdfGenerator _pdfGenerator;
    private readonly IRepository<Sale, SaleId> _saleRepository;
    private readonly IRepository<InvoiceTemplate, InvoiceTemplateId> _templateRepository;

    public GenerateInvoicePdfCommandHandler(
        IHtmlToPdfGenerator pdfGenerator,
        IRepository<Sale, SaleId> saleRepository,
        IRepository<InvoiceTemplate, InvoiceTemplateId> templateRepository)
    {
        _pdfGenerator = pdfGenerator;
        _saleRepository = saleRepository;
        _templateRepository = templateRepository;
    }

    public async Task<Result<byte[]>> Handle(GenerateInvoicePdfCommand request, CancellationToken cancellationToken)
    {
        var saleId = SaleId.From(request.SaleId);

        var sale = await _saleRepository.GetByIdAsync(saleId, cancellationToken);
        if (sale == null)
            return Error.NotFound("Sale.NotFound", "La venta especificada no existe.");

        InvoiceTemplate? template = null;
        if (request.InvoiceTemplateId == Guid.Empty)
        {
            var templates = await _templateRepository.FindAsync(t => t.CompanyId == sale.CompanyId, cancellationToken);
            template = templates.Count > 0 ? templates[0] : null;
        }
        else
        {
            var templateId = InvoiceTemplateId.From(request.InvoiceTemplateId);
            template = await _templateRepository.GetByIdAsync(templateId, cancellationToken);
        }
        if (template == null)
            return Error.NotFound("InvoiceTemplate.NotFound", "La plantilla de factura especificada no existe.");

        // Interpolación básica de variables (ponytail approach: simple string replace instead of heavy template engine like RazorLight)
        var html = template.HtmlContent
            .Replace("{{CustomerName}}", sale.CustomerName)
            .Replace("{{CustomerNit}}", sale.CustomerNit)
            .Replace("{{CustomerNrc}}", sale.CustomerNrc)
            .Replace("{{IssueDate}}", sale.IssueDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture))
            .Replace("{{DocumentNumber}}", sale.DocumentNumber)
            .Replace("{{TaxableAmount}}", sale.TaxableAmount.ToString("C2", CultureInfo.InvariantCulture))
            .Replace("{{ExemptAmount}}", sale.ExemptAmount.ToString("C2", CultureInfo.InvariantCulture))
            .Replace("{{TaxAmount}}", sale.TaxAmount.ToString("C2", CultureInfo.InvariantCulture))
            .Replace("{{TotalAmount}}", sale.TotalAmount.ToString("C2", CultureInfo.InvariantCulture));

        try
        {
            var pdfBytes = await _pdfGenerator.GeneratePdfAsync(html, cancellationToken);
            return pdfBytes;
        }
        catch (Exception ex)
        {
            return Error.Failure("GeneratePdf.Error", $"Error al renderizar el PDF: {ex.Message}");
        }
    }
}
