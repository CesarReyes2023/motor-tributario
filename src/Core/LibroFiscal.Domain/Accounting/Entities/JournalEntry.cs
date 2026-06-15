#pragma warning disable CS8618 // EF Core constructor bindings

using LibroFiscal.Domain.Accounting.Enumerations;
using LibroFiscal.Domain.Common.Ids;
using LibroFiscal.SharedKernel.Primitives;
using LibroFiscal.SharedKernel.Results;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LibroFiscal.Domain.Accounting.Entities;

/// <summary>
/// Represents a Double-Entry Journal Entry (Asiento o Póliza Contable).
/// </summary>
public sealed class JournalEntry : AggregateRoot<JournalEntryId>
{
    private readonly List<JournalEntryLine> _lines = new();

    public CompanyId CompanyId { get; private set; }
    public DateTimeOffset Date { get; private set; }
    public string Description { get; private set; }
    public JournalEntryStatus Status { get; private set; }
    
    /// <summary>
    /// Optional reference to the underlying document (e.g., PurchaseId or DteId).
    /// </summary>
    public string? ReferenceDocumentId { get; private set; }

    public IReadOnlyCollection<JournalEntryLine> Lines => _lines.AsReadOnly();

    private JournalEntry() { } // EF Core

    public static Result<JournalEntry> Create(
        CompanyId companyId, 
        DateTimeOffset date, 
        string description,
        string? referenceDocumentId = null)
    {
        if (string.IsNullOrWhiteSpace(description))
            return Error.Validation("JournalEntry.DescriptionEmpty", "El concepto del asiento no puede estar vacío.");

        return new JournalEntry
        {
            Id = JournalEntryId.New(),
            CompanyId = companyId,
            Date = date,
            Description = description,
            Status = JournalEntryStatus.Draft,
            ReferenceDocumentId = referenceDocumentId
        };
    }

    public Result AddLine(AccountId accountId, decimal debit, decimal credit)
    {
        if (Status != JournalEntryStatus.Draft)
            return Error.Conflict("JournalEntry.NotDraft", "Solo se pueden agregar líneas a un asiento en borrador.");

        if (debit < 0 || credit < 0)
            return Error.Validation("JournalEntry.NegativeAmount", "Los cargos y abonos no pueden ser negativos.");

        if (debit == 0 && credit == 0)
            return Error.Validation("JournalEntry.ZeroAmount", "La línea debe tener un cargo o abono mayor a cero.");

        if (debit > 0 && credit > 0)
            return Error.Validation("JournalEntry.BothAmounts", "Una misma línea no puede tener cargo y abono a la vez.");

        _lines.Add(new JournalEntryLine(Id, accountId, debit, credit));
        return Result.Success();
    }

    public Result Post()
    {
        if (Status != JournalEntryStatus.Draft)
            return Error.Conflict("JournalEntry.NotDraft", "El asiento no está en borrador.");

        if (_lines.Count < 2)
            return Error.Validation("JournalEntry.InsufficientLines", "Un asiento debe tener al menos dos líneas.");

        var totalDebit = _lines.Sum(l => l.Debit);
        var totalCredit = _lines.Sum(l => l.Credit);

        if (totalDebit != totalCredit)
            return Error.Validation("JournalEntry.Unbalanced", $"El asiento está descuadrado. Cargos: {totalDebit}, Abonos: {totalCredit}");

        Status = JournalEntryStatus.Posted;
        return Result.Success();
    }
}

public sealed class JournalEntryLine : Entity<Guid>
{
    public JournalEntryId JournalEntryId { get; private set; }
    public AccountId AccountId { get; private set; }
    
    /// <summary>Cargo</summary>
    public decimal Debit { get; private set; }
    
    /// <summary>Abono</summary>
    public decimal Credit { get; private set; }

    private JournalEntryLine() { } // EF Core

    internal JournalEntryLine(JournalEntryId journalEntryId, AccountId accountId, decimal debit, decimal credit)
    {
        Id = Guid.NewGuid();
        JournalEntryId = journalEntryId;
        AccountId = accountId;
        Debit = debit;
        Credit = credit;
    }
}
