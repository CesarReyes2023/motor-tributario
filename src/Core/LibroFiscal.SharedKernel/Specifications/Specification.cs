using System.Linq.Expressions;

namespace LibroFiscal.SharedKernel.Specifications;

/// <summary>
/// Base implementation of the Specification pattern with composable operators.
/// Supports both EF Core query translation (via Expression) and in-memory evaluation.
/// </summary>
/// <typeparam name="T">The type being evaluated.</typeparam>
public abstract class Specification<T> : ISpecification<T>
{
    private Func<T, bool>? _compiledExpression;

    /// <summary>
    /// Override in derived classes to define the specification's predicate.
    /// </summary>
    public abstract Expression<Func<T, bool>> ToExpression();

    /// <summary>
    /// Evaluates the specification against an entity in memory.
    /// The expression is compiled and cached on first use.
    /// </summary>
    public bool IsSatisfiedBy(T entity)
    {
        _compiledExpression ??= ToExpression().Compile();
        return _compiledExpression(entity);
    }

    public ISpecification<T> And(ISpecification<T> other) =>
        new AndSpecification<T>(this, other);

    public ISpecification<T> Or(ISpecification<T> other) =>
        new OrSpecification<T>(this, other);

    public ISpecification<T> Not() =>
        new NotSpecification<T>(this);

    /// <summary>
    /// Implicit conversion to Expression for LINQ queries.
    /// </summary>
    public static implicit operator Expression<Func<T, bool>>(Specification<T> spec) =>
        spec.ToExpression();
}

/// <summary>
/// Combines two specifications with logical AND.
/// </summary>
internal sealed class AndSpecification<T> : Specification<T>
{
    private readonly ISpecification<T> _left;
    private readonly ISpecification<T> _right;

    public AndSpecification(ISpecification<T> left, ISpecification<T> right)
    {
        _left = left;
        _right = right;
    }

    public override Expression<Func<T, bool>> ToExpression()
    {
        var leftExpr = _left.ToExpression();
        var rightExpr = _right.ToExpression();

        var parameter = Expression.Parameter(typeof(T), "x");
        var body = Expression.AndAlso(
            Expression.Invoke(leftExpr, parameter),
            Expression.Invoke(rightExpr, parameter));

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }
}

/// <summary>
/// Combines two specifications with logical OR.
/// </summary>
internal sealed class OrSpecification<T> : Specification<T>
{
    private readonly ISpecification<T> _left;
    private readonly ISpecification<T> _right;

    public OrSpecification(ISpecification<T> left, ISpecification<T> right)
    {
        _left = left;
        _right = right;
    }

    public override Expression<Func<T, bool>> ToExpression()
    {
        var leftExpr = _left.ToExpression();
        var rightExpr = _right.ToExpression();

        var parameter = Expression.Parameter(typeof(T), "x");
        var body = Expression.OrElse(
            Expression.Invoke(leftExpr, parameter),
            Expression.Invoke(rightExpr, parameter));

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }
}

/// <summary>
/// Negates a specification.
/// </summary>
internal sealed class NotSpecification<T> : Specification<T>
{
    private readonly ISpecification<T> _inner;

    public NotSpecification(ISpecification<T> inner)
    {
        _inner = inner;
    }

    public override Expression<Func<T, bool>> ToExpression()
    {
        var innerExpr = _inner.ToExpression();

        var parameter = Expression.Parameter(typeof(T), "x");
        var body = Expression.Not(Expression.Invoke(innerExpr, parameter));

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }
}
