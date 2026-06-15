using LibroFiscal.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace TestDb;

sealed class Program
{
    static void Main(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<LibroFiscalDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=librofiscal;Username=postgres;Password=zeref");
        
        using var db = new LibroFiscalDbContext(optionsBuilder.Options);

        Console.WriteLine("---- RESULTADOS DE LA BASE DE DATOS POSTGRESQL ----");
        
        var purchasesCount = db.Purchases.Count();
        Console.WriteLine($"Total de Compras registradas: {purchasesCount}");

        if (purchasesCount > 0)
        {
            var lastPurchase = db.Purchases.OrderBy(p => p.RecordedAt).Last();
            Console.WriteLine($"Última compra: Proveedor={lastPurchase.SupplierName}, Total=${lastPurchase.TotalAmount}");
        }

        var journalEntriesCount = db.JournalEntries.Count();
        Console.WriteLine($"Total de Pólizas (Asientos) registradas: {journalEntriesCount}");

        var accountsCount = db.Accounts.Count();
        Console.WriteLine($"Total de Cuentas en el Catálogo: {accountsCount}");
        
        if (accountsCount > 0)
        {
            Console.WriteLine("Cuentas auto-generadas encontradas:");
            foreach (var account in db.Accounts.ToList())
            {
                Console.WriteLine($" - {account.Code}: {account.Name} ({account.Type})");
            }
        }
        
        Console.WriteLine("---------------------------------------------------");
    }
}
