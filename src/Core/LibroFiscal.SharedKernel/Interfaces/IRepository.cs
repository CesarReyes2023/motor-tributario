using System.Linq.Expressions;
using LibroFiscal.SharedKernel.Primitives;
using LibroFiscal.SharedKernel.Specifications;

namespace LibroFiscal.SharedKernel.Interfaces;

/// <summary>
/// Generic repository interface for aggregate roots.
/// Repositories are the persistence boundary — only aggregate roots get repositories.
/// 
/// This interface defines the contract; implementations live in Infrastructure.Persistence.
/// The Application layer depends on this interface, never on the EF Core implementation.
/// </summary>
/// <typeparam name="TEntity">The aggregate root type.</typeparam>
/// <typeparam name="TId">The strongly-typed ID type.</typeparam>
public interface IRepository<TEntity, in TId>
    where TEntity : AggregateRoot<TId>
    where TId : notnull
{
    /// <summary>
    /// Finds an aggregate by its ID. Returns null if not found.
    /// </summary>
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all entities matching the specification.
    /// </summary>
    Task<IReadOnlyList<TEntity>> GetBySpecAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the first entity matching the specification, or null.
    /// </summary>
    Task<TEntity?> FirstOrDefaultAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all entities matching the predicate.
    /// </summary>
    Task<IReadOnlyList<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all entities matching the predicate, with ordering.
    /// </summary>
    Task<IReadOnlyList<TEntity>> FindAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if any entity matches the specification.
    /// </summary>
    Task<bool> AnyAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts entities matching the specification.
    /// </summary>
    Task<int> CountAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new aggregate to the repository.
    /// </summary>
    void Add(TEntity entity);

    /// <summary>
    /// Adds a collection of aggregates to the repository.
    /// </summary>
    void AddRange(IEnumerable<TEntity> entities);

    /// <summary>
    /// Marks an aggregate as modified.
    /// </summary>
    void Update(TEntity entity);

    /// <summary>
    /// Removes an aggregate from the repository.
    /// </summary>
    void Remove(TEntity entity);
}
