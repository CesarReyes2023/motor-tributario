using System.Linq.Expressions;

#pragma warning disable CA1716 // Method names And/Or/Not conflict with reserved keywords — intentional: standard Specification pattern naming

namespace LibroFiscal.SharedKernel.Specifications;

/// <summary>
/// Specification pattern interface. Encapsulates a business rule as a reusable,
/// composable predicate that can be combined with And/Or/Not operators.
/// 
/// Used extensively by the Fiscal Engine for tax rule applicability checks.
/// Also used for query filtering in repositories.
/// </summary>
/// <typeparam name="T">The type being evaluated by the specification.</typeparam>
public interface ISpecification<T>
{
    /// <summary>
    /// Expression form — can be translated to SQL by EF Core for server-side filtering.
    /// </summary>
    Expression<Func<T, bool>> ToExpression();

    /// <summary>
    /// Compiled delegate for in-memory evaluation.
    /// </summary>
    bool IsSatisfiedBy(T entity);

    /// <summary>
    /// Combines this specification with another using logical AND.
    /// </summary>
    ISpecification<T> And(ISpecification<T> other);

    /// <summary>
    /// Combines this specification with another using logical OR.
    /// </summary>
    ISpecification<T> Or(ISpecification<T> other);

    /// <summary>
    /// Negates this specification.
    /// </summary>
    ISpecification<T> Not();
}
