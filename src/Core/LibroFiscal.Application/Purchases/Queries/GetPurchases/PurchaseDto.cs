using System;

namespace LibroFiscal.Application.Purchases.Queries.GetPurchases;

public sealed record PurchaseDto(
    Guid Id,
    string SupplierNit,
    string SupplierName,
    DateTimeOffset IssueDate,
    string DocumentNumber,
    decimal SubTotal,
    decimal TaxAmount,
    decimal TotalAmount,
    Guid? JournalEntryId);
