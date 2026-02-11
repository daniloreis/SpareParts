using Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

public abstract class ServiceBase<T> : IServiceBase<T> where T : class
{
    protected IRepositoryBase<T> repositoryBase;

    public ServiceBase(IRepositoryBase<T> repository)
    {
        repositoryBase = repository;
    }

    private object GetKey(T obj)
    {
        foreach (var prop in obj.GetType().GetProperties())
        {
            if (prop.GetCustomAttributes(typeof(KeyAttribute), true).Any())
                return prop.GetValue(obj);
        }

        return null;
    }

    public void Save(T obj)
    {
        try
        {
            if (obj != null)
            {
                object id = GetKey(obj);

                if (id != null)
                    repositoryBase.Update(obj);
                else
                    repositoryBase.Add(obj);
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public void AddRange(IEnumerable<T> collection)
    {
        try
        {
            repositoryBase.AddRange(collection);
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
            repositoryBase.Remove(obj);
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
            return repositoryBase.FindById(keyValues);
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
            return repositoryBase.FindAll();
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
}

