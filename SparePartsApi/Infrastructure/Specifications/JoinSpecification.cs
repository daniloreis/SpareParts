namespace Domain
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;

    internal class JoinSpecification<T> : ISpecification<T>
    {
        private readonly ISpecification<T> left;
        private readonly ISpecification<T> right;

        public JoinSpecification(ISpecification<T> left, ISpecification<T> right)
        {
            this.left = left;
            this.right = right;
        }

        public Expression<Func<T, bool>> IsSatisifiedBy()
        {
            var leftExpression = left.IsSatisifiedBy();
            var rightExpression = right.IsSatisifiedBy();

            var parameter = leftExpression.Parameters.Single();
            var body = Expression.Add(leftExpression.Body, SpecificationParameterRebinder.ReplaceParameter(rightExpression.Body, parameter));

            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }
    }
}