namespace LibroFiscal.Domain.Accounting.Enumerations;

public enum AccountType
{
    Asset = 1,      // Activo
    Liability = 2,  // Pasivo
    Equity = 3,     // Capital
    Revenue = 4,    // Ingreso
    Expense = 5     // Gasto
}

public enum JournalEntryStatus
{
    Draft = 1,      // Borrador (Editable)
    Posted = 2,     // Mayorizado / Contabilizado (Definitivo)
    Voided = 3      // Anulado
}
