#pragma warning disable CS8618 // EF Core constructor bindings

using LibroFiscal.Domain.Common.Ids;
using LibroFiscal.SharedKernel.Primitives;
using LibroFiscal.SharedKernel.Results;
using System;

namespace LibroFiscal.Domain.Companies.Entities;

/// <summary>
/// Represents a custom HTML template for printing invoices (DTEs / Ventas).
/// Used by the WebView2 PDF generator to render the physical documents.
/// </summary>
public sealed class InvoiceTemplate : AggregateRoot<InvoiceTemplateId>
{
    public CompanyId CompanyId { get; private set; }
    
    /// <summary>
    /// The name of the template (e.g. "Factura Consumidor Final", "CCF").
    /// </summary>
    public string Name { get; private set; }
    
    /// <summary>
    /// The raw HTML string with {{Variables}} for data binding.
    /// </summary>
    public string HtmlContent { get; private set; }
    
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    private InvoiceTemplate() { } // EF Core

    public static Result<InvoiceTemplate> Create(
        CompanyId companyId,
        string name,
        string htmlContent)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Error.Validation("InvoiceTemplate.NoName", "El nombre de la plantilla es requerido.");

        if (string.IsNullOrWhiteSpace(htmlContent))
            return Error.Validation("InvoiceTemplate.NoHtml", "El contenido HTML no puede estar vacío.");

        return new InvoiceTemplate
        {
            Id = InvoiceTemplateId.New(),
            CompanyId = companyId,
            Name = name,
            HtmlContent = htmlContent,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public Result UpdateContent(string newHtmlContent)
    {
        if (string.IsNullOrWhiteSpace(newHtmlContent))
            return Error.Validation("InvoiceTemplate.NoHtml", "El contenido HTML no puede estar vacío.");

        HtmlContent = newHtmlContent;
        UpdatedAt = DateTimeOffset.UtcNow;
        
        return Result.Success();
    }
}
