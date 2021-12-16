using System;
using System.Collections.Generic;
using System.Linq;
using Moneyman.Domain;
using Microsoft.EntityFrameworkCore;
using Moneyman.Interfaces;
using Moneyman.Models;

namespace Moneyman.Persistence
{
  public class GenericRepository<T> : IRepository<T> where T : class
  {
    protected MoneymanContext _context;

    public GenericRepository(MoneymanContext context)
    {
        _context = context;
    }
    public virtual void Add(T newObject)
    {
        _context.Set<T>().Add(newObject);
    }

    public virtual T Get(int id)
    {
      return _context.Set<T>().Find(id);
    }

    public virtual IEnumerable<T> GetAll()
    {
      return  _context.Set<T>().AsEnumerable();
    }

    public virtual DbSet<T> GetDbSet()
    {
      return _context.Set<T>();
    }

    public virtual void Remove(int id)
    {
        var entity = Get(id);
        _context.Set<T>().Remove(entity);
    }

    public virtual bool Update(T newObject, int id)
    {
        var existing = _context.Set<T>().Find(id);
        if (existing == null)
        {
            _context.Add(newObject);
            return true;
        }

        _context.Entry(existing).CurrentValues.SetValues(newObject);

        return true; //TODO - Return failure state
    }

    public virtual int Save()
    {
        return _context.SaveChanges();
    }
  }
}
