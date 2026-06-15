using System;

namespace LibroFiscal.Application.Taxes.Queries.GetVatPurchasesBook;

public sealed record VatPurchaseDto(
    Guid Id,
    int RowNumber,
    DateTimeOffset IssueDate,
    string DocumentNumber,
    string SupplierNit,
    string SupplierNrc,
    string SupplierName,
    decimal ExemptPurchases,
    decimal InternalPurchases,
    decimal Imports,
    decimal TaxCredit,
    decimal RetainedTax,
    decimal ExcludedPurchases,
    decimal TotalPurchases);
