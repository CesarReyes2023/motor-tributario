#pragma warning disable CS8618 // EF Core constructor bindings

using LibroFiscal.Domain.Accounting.Enumerations;
using LibroFiscal.Domain.Common.Ids;
using LibroFiscal.SharedKernel.Primitives;
using LibroFiscal.SharedKernel.Results;
using System.Collections.Generic;

namespace LibroFiscal.Domain.Accounting.Entities;

/// <summary>
/// Represents an account in the Chart of Accounts (Catálogo de Cuentas).
/// Organized hierarchically using ParentAccountId.
/// </summary>
public sealed class Account : AggregateRoot<AccountId>
{
    private readonly List<Account> _subAccounts = new();

    public CompanyId CompanyId { get; private set; }
    public string Code { get; private set; }
    public string Name { get; private set; }
    public AccountType Type { get; private set; }
    public AccountId? ParentAccountId { get; private set; }
    
    // Navigation property for EF Core
    public Account? ParentAccount { get; private set; }
    public IReadOnlyCollection<Account> SubAccounts => _subAccounts.AsReadOnly();

    /// <summary>
    /// Indicates if this account can receive transactions directly.
    /// Typically, only lowest-level accounts (Rubros de detalle) accept transactions.
    /// </summary>
    public bool IsTransactional { get; private set; }

    private Account() { } // EF Core

    public static Result<Account> Create(
        CompanyId companyId, 
        string code, 
        string name, 
        AccountType type, 
        bool isTransactional,
        AccountId? parentAccountId = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            return Error.Validation("Account.CodeEmpty", "El código de la cuenta es requerido.");
            
        if (string.IsNullOrWhiteSpace(name))
            return Error.Validation("Account.NameEmpty", "El nombre de la cuenta es requerido.");

        return new Account
        {
            Id = AccountId.New(),
            CompanyId = companyId,
            Code = code,
            Name = name,
            Type = type,
            IsTransactional = isTransactional,
            ParentAccountId = parentAccountId
        };
    }
}
