#pragma warning disable CS8618 // EF Core constructor bindings

using LibroFiscal.Domain.Common.Ids;
using LibroFiscal.SharedKernel.Primitives;
using LibroFiscal.SharedKernel.Results;
using System;

namespace LibroFiscal.Domain.Purchases.Entities;

/// <summary>
/// Represents a received invoice/purchase (Compra / Documento Recibido).
/// This serves as the source document for Accounts Payable and Journal Entries.
/// </summary>
public sealed class Purchase : AggregateRoot<PurchaseId>
{
    public CompanyId CompanyId { get; private set; }
    
    // Supplier Information
    public string SupplierNit { get; private set; }
    public string SupplierNrc { get; private set; }
    public string SupplierName { get; private set; }
    
    // Document Information
    public DateTimeOffset IssueDate { get; private set; }
    public string DocumentNumber { get; private set; } // Factura #, CCF #, etc.
    
    // Amounts
    public decimal SubTotal { get; private set; }
    public decimal TaxAmount { get; private set; } // IVA
    public decimal TotalAmount { get; private set; }
    
    // Audit
    public DateTimeOffset RecordedAt { get; private set; }
    
    /// <summary>
    /// Link to the generated Journal Entry.
    /// </summary>
    public JournalEntryId? JournalEntryId { get; private set; }

    private Purchase() { } // EF Core

    public static Result<Purchase> Create(
        CompanyId companyId,
        string supplierNit,
        string supplierNrc,
        string supplierName,
        DateTimeOffset issueDate,
        string documentNumber,
        decimal subTotal,
        decimal taxAmount,
        decimal totalAmount)
    {
        if (string.IsNullOrWhiteSpace(supplierNit))
            return Error.Validation("Purchase.NoNit", "El NIT del proveedor es requerido.");

        if (totalAmount <= 0)
            return Error.Validation("Purchase.InvalidTotal", "El total de la compra debe ser mayor a cero.");

        return new Purchase
        {
            Id = PurchaseId.New(),
            CompanyId = companyId,
            SupplierNit = supplierNit,
            SupplierNrc = supplierNrc,
            SupplierName = supplierName,
            IssueDate = issueDate,
            DocumentNumber = documentNumber,
            SubTotal = subTotal,
            TaxAmount = taxAmount,
            TotalAmount = totalAmount,
            RecordedAt = DateTimeOffset.UtcNow
        };
    }

    public Result LinkJournalEntry(JournalEntryId journalEntryId)
    {
        if (JournalEntryId != null)
            return Error.Conflict("Purchase.AlreadyLinked", "La compra ya tiene un asiento contable enlazado.");

        JournalEntryId = journalEntryId;
        return Result.Success();
    }
}
