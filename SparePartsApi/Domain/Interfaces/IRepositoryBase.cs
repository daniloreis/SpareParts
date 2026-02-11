using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Domain
{
    public interface IRepositoryBase<T> where T : class
    {
        T Add(T obj);
        IEnumerable<T> AddRange(IEnumerable<T> collection);
        void Update(T obj);
        void UpdateRange(IEnumerable<T> collection);
        void Remove(T obj);
        void RemoveRange(IEnumerable<T> collection);
        T FindById(params object[] keyValues);
        IEnumerable<T> FindAll();
        IEnumerable<T> FindByCriteria(Expression<Func<T, bool>> predicate);
        IEnumerable<T> FindBySpec(ISpecification<T> spec);
        IEnumerable<T> FindByQuery(string query, params object[] obj);
    }
}
