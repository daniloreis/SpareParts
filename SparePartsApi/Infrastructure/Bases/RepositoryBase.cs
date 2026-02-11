using Domain;
using Infrastructure;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

public abstract class RepositoryBase<T> : IRepositoryBase<T> where T : class
{
    protected DataContext dbContext;

    public RepositoryBase()
    {
        if (dbContext == null)
            dbContext = new DataContext();

        dbContext.Set<T>().AsNoTracking();
    }
    public T Add(T obj)
    {
        try
        {
            obj = dbContext.Set<T>().Add(obj);
            dbContext.SaveChanges();
            return obj;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    public IEnumerable<T> AddRange(IEnumerable<T> collection)
    {
        try
        {
            collection = dbContext.Set<T>().AddRange(collection);
            dbContext.SaveChanges();
            return collection;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    public void Update(T obj)
    {
        try
        {
            dbContext.Entry(obj).State = EntityState.Modified;
            dbContext.SaveChanges();
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    public void UpdateRange(IEnumerable<T> collection)
    {
        try
        {
            foreach (var obj in collection)
                dbContext.Entry(obj).State = EntityState.Modified;

            dbContext.SaveChanges();
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    public void Remove(T obj)
    {
        try
        {
            dbContext.Set<T>().Remove(obj);
            dbContext.SaveChanges();
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    public void RemoveRange(IEnumerable<T> collection)
    {
        try
        {
            dbContext.Set<T>().RemoveRange(collection);
            dbContext.SaveChanges();
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    public T FindById(params object[] keyValues)
    {
        try
        {
            return dbContext.Set<T>().Find(keyValues);
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    public IEnumerable<T> FindAll()
    {
        try
        {
            return dbContext.Set<T>().AsEnumerable();
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    public IEnumerable<T> FindByCriteria(Expression<Func<T, bool>> predicate)
    {
        try
        {
            return dbContext.Set<T>().Where(predicate).AsEnumerable();
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    public IEnumerable<T> FindBySpec(ISpecification<T> spec)
    {
        try
        {
            return dbContext.Set<T>().Where(spec.IsSatisifiedBy()).AsEnumerable();
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    public IEnumerable<T> FindByQuery(string query, params object[] obj)
    {
        try
        {
            return dbContext.Set<T>().SqlQuery(query, obj).AsEnumerable();
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

}
