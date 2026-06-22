#pragma warning disable CS8618 // EF Core constructor bindings

using LibroFiscal.Domain.Common.Ids;
using LibroFiscal.SharedKernel.Primitives;
using LibroFiscal.SharedKernel.Results;
using System;

namespace LibroFiscal.Domain.Sales.Entities;

/// <summary>
/// Represents an issued invoice/sale (Venta / Documento Emitido).
/// This serves as the source document for Accounts Receivable and the Sales Book.
/// </summary>
public sealed class Sale : AggregateRoot<SaleId>
{
    public CompanyId CompanyId { get; private set; }
    
    // Customer Information
    public string CustomerNit { get; private set; }
    public string CustomerNrc { get; private set; }
    public string CustomerName { get; private set; }
    
    // Document Information
    public DateTimeOffset IssueDate { get; private set; }
    public string DocumentNumber { get; private set; } // Factura #, CCF #, etc.
    
    // Amounts
    public decimal TaxableAmount { get; private set; } // Ventas Gravadas
    public decimal ExemptAmount { get; private set; } // Ventas Exentas
    public decimal TaxAmount { get; private set; } // IVA
    public decimal TotalAmount { get; private set; }
    
    // Audit
    public DateTimeOffset RecordedAt { get; private set; }
    
    /// <summary>
    /// Link to the generated Journal Entry.
    /// </summary>
    public JournalEntryId? JournalEntryId { get; private set; }

    private Sale() { } // EF Core

    public static Result<Sale> Create(
        CompanyId companyId,
        string customerNit,
        string customerNrc,
        string customerName,
        DateTimeOffset issueDate,
        string documentNumber,
        decimal taxableAmount,
        decimal exemptAmount,
        decimal taxAmount,
        decimal totalAmount)
    {
        if (string.IsNullOrWhiteSpace(customerName))
            return Error.Validation("Sale.NoCustomer", "El nombre del cliente es requerido.");

        if (totalAmount <= 0)
            return Error.Validation("Sale.InvalidTotal", "El total de la venta debe ser mayor a cero.");

        return new Sale
        {
            Id = SaleId.New(),
            CompanyId = companyId,
            CustomerNit = customerNit,
            CustomerNrc = customerNrc,
            CustomerName = customerName,
            IssueDate = issueDate,
            DocumentNumber = documentNumber,
            TaxableAmount = taxableAmount,
            ExemptAmount = exemptAmount,
            TaxAmount = taxAmount,
            TotalAmount = totalAmount,
            RecordedAt = DateTimeOffset.UtcNow
        };
    }

    public Result LinkJournalEntry(JournalEntryId journalEntryId)
    {
        if (JournalEntryId != null)
            return Error.Conflict("Sale.AlreadyLinked", "La venta ya tiene un asiento contable enlazado.");

        JournalEntryId = journalEntryId;
        return Result.Success();
    }
}
