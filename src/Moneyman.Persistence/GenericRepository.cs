using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Moneyman.Domain;
using Microsoft.EntityFrameworkCore;
using Moneyman.Interfaces;
using System.Threading.Tasks;
using Moneyman.Domain.Interfaces;

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

    public virtual void Remove(int id)
    {
        var entity = Get(id);
        _context.Set<T>().Remove(entity);
    }

    public virtual bool Update(T newObject)
    {
      IEntity entity = (IEntity)newObject;

      var existing = _context.Set<T>().Find(entity.Id);

      if (existing == null)
      {

          return false;
      }
      _context.Entry(existing).CurrentValues.SetValues(newObject);
      _context.SaveChanges();

      return true;
    }

    public virtual async Task<int>  Save()
    {
        return await _context.SaveChangesAsync();
    }

    //TODO: Update this to accept T
    public bool RemoveAll(string tableName)
    {
        // A table name cannot be supplied as a SQL parameter, so guard against
        // injection by accepting only simple identifiers. Callers pass
        // compile-time constants ("PlanDates", "Transactions", "Paydays").
        if (string.IsNullOrWhiteSpace(tableName) || !Regex.IsMatch(tableName, "^[A-Za-z0-9_]+$"))
        {
            throw new ArgumentException($"Invalid table name '{tableName}'.", nameof(tableName));
        }

#pragma warning disable EF1002 // tableName validated as a safe identifier above; it is never user input.
        var result = _context.Database.ExecuteSqlRaw($"DELETE FROM {tableName}");
#pragma warning restore EF1002
        return result == 1;
    }
  }
}
