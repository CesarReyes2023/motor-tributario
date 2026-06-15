namespace LibroFiscal.SharedKernel.Interfaces;

/// <summary>
/// Unit of Work pattern. Coordinates the persistence of changes across multiple repositories
/// within a single transaction. EF Core's DbContext is the natural implementation.
/// 
/// The UoW ensures that either all changes in a business operation are saved,
/// or none are — critical for fiscal data integrity (e.g., creating a DTE and its audit entry atomically).
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Persists all pending changes and dispatches domain events.
    /// Returns the number of state entries written to the database.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
