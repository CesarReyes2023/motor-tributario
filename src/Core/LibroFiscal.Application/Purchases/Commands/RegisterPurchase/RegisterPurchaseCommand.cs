using LibroFiscal.Application.Abstractions.Messaging;
using LibroFiscal.Domain.Common.Ids;
using System;

namespace LibroFiscal.Application.Purchases.Commands.RegisterPurchase;

public sealed record RegisterPurchaseCommand(
    Guid CompanyId,
    string SupplierNit,
    string SupplierNrc,
    string SupplierName,
    DateTimeOffset IssueDate,
    string DocumentNumber,
    decimal SubTotal,
    decimal TaxAmount,
    decimal TotalAmount
) : ICommand<PurchaseId>;
