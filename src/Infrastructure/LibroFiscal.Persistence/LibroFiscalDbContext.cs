using LibroFiscal.Domain.Companies.Entities;
using LibroFiscal.Domain.DTE.Entities;
using LibroFiscal.Domain.FiscalBooks.Entities;
using LibroFiscal.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore;

using LibroFiscal.Domain.Users.Entities;
using LibroFiscal.SharedKernel.Primitives;
using LibroFiscal.Application.Abstractions.Services;

using LibroFiscal.Domain.Taxes.Entities;
using LibroFiscal.Domain.Accounting.Entities;
using LibroFiscal.Domain.Purchases.Entities;
using LibroFiscal.Domain.Sales.Entities;

namespace LibroFiscal.Persistence;

public sealed class LibroFiscalDbContext : DbContext, IUnitOfWork
{
    private readonly ICurrentUserService? _currentUserService;

    public LibroFiscalDbContext(DbContextOptions<LibroFiscalDbContext> options, ICurrentUserService? currentUserService = null)
        : base(options)
    {
        _currentUserService = currentUserService;
    }

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<DteDocument> Dtes => Set<DteDocument>();
    public DbSet<LibroIva> LibrosIva => Set<LibroIva>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserCompanyAccess> UserCompanyAccesses => Set<UserCompanyAccess>();
    public DbSet<TaxRule> TaxRules => Set<TaxRule>();
    
    // Phase 12: Accounting & Purchases
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
    public DbSet<Purchase> Purchases => Set<Purchase>();

    // Phase 13: Sales & Custom Invoices
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<InvoiceTemplate> InvoiceTemplates => Set<InvoiceTemplate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LibroFiscalDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var userId = _currentUserService?.Username ?? "System";

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is not IAuditableEntity auditableEntity)
                continue;

            switch (entry.State)
            {
                case EntityState.Added:
                    auditableEntity.SetCreatedAudit(now, userId);
                    break;
                case EntityState.Modified:
                    auditableEntity.SetModifiedAudit(now, userId);
                    break;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
