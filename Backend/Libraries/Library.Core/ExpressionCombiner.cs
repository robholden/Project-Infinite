using System.Linq.Expressions;

namespace Library.Core;

/// <summary>
/// <para>Combines two lambda expressions</para>
/// <para>Source: https://gist.github.com/janvanderhaegen/1620fee98ea8578862aa</para>
/// </summary>
public static class ExpressionCombiner
{
    /// <summary>
    /// Ands the specified new exp.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="exp">The exp.</param>
    /// <param name="newExp">The new exp.</param>
    /// <returns></returns>
    public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> exp, Expression<Func<T, bool>> newExp)
    {
        // get the visitor
        ParameterUpdateVisitor visitor = new(newExp.Parameters[0], exp.Parameters[0]);

        // replace the parameter in the expression just created
        newExp = visitor.Visit(newExp) as Expression<Func<T, bool>>;

        // now you can and together the two expressions
        BinaryExpression binExp = Expression.And(exp.Body, newExp.Body);

        // and return a new lambda, that will do what you want. NOTE that the binExp has
        // reference only to te newExp.Parameters[0] (there is only 1) parameter, and no other
        return Expression.Lambda<Func<T, bool>>(binExp, newExp.Parameters);
    }

    private class ParameterUpdateVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression _newParameter;
        private readonly ParameterExpression _oldParameter;

        public ParameterUpdateVisitor(ParameterExpression oldParameter, ParameterExpression newParameter)
        {
            _oldParameter = oldParameter;
            _newParameter = newParameter;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (object.ReferenceEquals(node, _oldParameter))
            {
                return _newParameter;
            }

            return base.VisitParameter(node);
        }
    }
}