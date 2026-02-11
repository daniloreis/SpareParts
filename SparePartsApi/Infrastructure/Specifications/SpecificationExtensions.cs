namespace Domain
{
    public static class SpecificationExtensions
    {
        public static bool IsSatisfiedBy<T>(this ISpecification<T> specification, T entity)
        {
            return specification.IsSatisifiedBy().Compile()(entity);
        }

        public static ISpecification<T> And<T>(this ISpecification<T> left, ISpecification<T> right)
        {
            return Specification.And(left, right);
        }

        public static ISpecification<T> Or<T>(this ISpecification<T> left, ISpecification<T> right)
        {
            return Specification.Or(left, right);
        }

        public static ISpecification<T> Join<T>(this ISpecification<T> left, ISpecification<T> right)
        {
            return Specification.Join(left, right);
        }
    }
}