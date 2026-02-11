using System.Collections.Generic;

namespace Domain
{
    public interface IServiceBase<T> where T : class
    {
        void Save(T obj);
        void Remove(T obj);
        T FindById(params object[] keyValues);
        IEnumerable<T> FindAll();
    }
}
