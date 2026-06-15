using LibroFiscal.Application.Abstractions.Messaging;
using LibroFiscal.Domain.Accounting.Entities;
using LibroFiscal.Domain.Accounting.Enumerations;
using LibroFiscal.Domain.Common.Ids;
using LibroFiscal.Domain.Purchases.Entities;
using LibroFiscal.SharedKernel.Interfaces;
using LibroFiscal.SharedKernel.Results;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LibroFiscal.Application.Purchases.Commands.RegisterPurchase;

internal sealed class RegisterPurchaseCommandHandler : ICommandHandler<RegisterPurchaseCommand, PurchaseId>
{
    private readonly IRepository<Purchase, PurchaseId> _purchaseRepository;
    private readonly IRepository<Account, AccountId> _accountRepository;
    private readonly IRepository<JournalEntry, JournalEntryId> _journalEntryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterPurchaseCommandHandler(
        IRepository<Purchase, PurchaseId> purchaseRepository,
        IRepository<Account, AccountId> accountRepository,
        IRepository<JournalEntry, JournalEntryId> journalEntryRepository,
        IUnitOfWork unitOfWork)
    {
        _purchaseRepository = purchaseRepository;
        _accountRepository = accountRepository;
        _journalEntryRepository = journalEntryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PurchaseId>> Handle(RegisterPurchaseCommand request, CancellationToken cancellationToken)
    {
        var companyId = CompanyId.From(request.CompanyId);

        // 1. Create Purchase
        var purchaseResult = Purchase.Create(
            companyId,
            request.SupplierNit,
            request.SupplierNrc ?? string.Empty,
            request.SupplierName,
            request.IssueDate,
            request.DocumentNumber,
            request.SubTotal,
            request.TaxAmount,
            request.TotalAmount);

        if (purchaseResult.IsFailure)
            return Result.Failure<PurchaseId>(purchaseResult.Error);

        var purchase = purchaseResult.Value;
        _purchaseRepository.Add(purchase);

        // 2. Ensure standard accounts exist for this company (MVP Auto-Seed)
        var expenseAccount = await GetOrCreateAccountAsync(companyId, "501", "Gasto por Compras", AccountType.Expense, cancellationToken);
        var ivaAccount = await GetOrCreateAccountAsync(companyId, "105", "IVA Crédito Fiscal", AccountType.Asset, cancellationToken);
        var payableAccount = await GetOrCreateAccountAsync(companyId, "201", "Cuentas por Pagar Proveedores", AccountType.Liability, cancellationToken);

        // 3. Create Journal Entry
        var journalEntryResult = JournalEntry.Create(
            companyId,
            request.IssueDate, // Usually recorded on issue date or received date
            $"Compra a {request.SupplierName} (Factura/CCF {request.DocumentNumber})",
            purchase.Id.Value.ToString());

        if (journalEntryResult.IsFailure)
            return Result.Failure<PurchaseId>(journalEntryResult.Error);

        var journalEntry = journalEntryResult.Value;

        // 4. Add lines
        // Debit Expense (Subtotal)
        var line1 = journalEntry.AddLine(expenseAccount.Id, debit: purchase.SubTotal, credit: 0);
        if (line1.IsFailure) return Result.Failure<PurchaseId>(line1.Error);

        // Debit IVA (Tax)
        if (purchase.TaxAmount > 0)
        {
            var line2 = journalEntry.AddLine(ivaAccount.Id, debit: purchase.TaxAmount, credit: 0);
            if (line2.IsFailure) return Result.Failure<PurchaseId>(line2.Error);
        }

        // Credit Payable (Total)
        var line3 = journalEntry.AddLine(payableAccount.Id, debit: 0, credit: purchase.TotalAmount);
        if (line3.IsFailure) return Result.Failure<PurchaseId>(line3.Error);

        // 5. Post Journal Entry
        var postResult = journalEntry.Post();
        if (postResult.IsFailure)
            return Result.Failure<PurchaseId>(postResult.Error);

        _journalEntryRepository.Add(journalEntry);

        // 6. Link
        purchase.LinkJournalEntry(journalEntry.Id);

        // 7. Save
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return purchase.Id;
    }

    private async Task<Account> GetOrCreateAccountAsync(
        CompanyId companyId, 
        string code, 
        string name, 
        AccountType type, 
        CancellationToken cancellationToken)
    {
        var accounts = await _accountRepository.FindAsync(a => a.CompanyId == companyId && a.Code == code, cancellationToken);
        var account = accounts.Count > 0 ? accounts[0] : null;

        if (account == null)
        {
            var result = Account.Create(companyId, code, name, type, isTransactional: true);
            account = result.Value;
            _accountRepository.Add(account);
            
            // We don't SaveChangesAsync here, we let the main transaction handle it
        }

        return account;
    }
}
