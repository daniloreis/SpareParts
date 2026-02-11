
namespace Domain
{
    public static class Specification
    {
        public static ISpecification<T> And<T>(ISpecification<T> left, ISpecification<T> right)
        {
            return new AndSpecification<T>(left, right);
        }

        public static ISpecification<T> Or<T>(ISpecification<T> left, ISpecification<T> right)
        {
            return new OrSpecification<T>(left, right);
        }

        public static ISpecification<T> Not<T>(ISpecification<T> specification)
        {
            return new NotSpecification<T>(specification);
        }

        public static ISpecification<T> Join<T>(ISpecification<T> left, ISpecification<T> right)
        {
            return new JoinSpecification<T>(left, right);
        }
    }
}