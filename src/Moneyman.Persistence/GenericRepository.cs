using System.Collections.Generic;
using System.Linq;
using Moneyman.Domain;
using Microsoft.EntityFrameworkCore;
using Moneyman.Interfaces;
using System.Threading.Tasks;
using AutoMapper;

namespace Moneyman.Persistence
{
  public class GenericRepository<T> : IRepository<T> where T : class
  {
    protected MoneymanContext _context;
    protected IMapper _mapper;

    public GenericRepository(MoneymanContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
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

    public virtual bool Update(T newObject)
    {
      IEntity entity = (IEntity)newObject;

      var existing = _context.Set<T>().Find(entity.Id);
       _mapper.Map(newObject, existing);
        
      if (existing == null)
      {
          _context.Add(newObject);
          return true;
      }

      //_context.Entry(existing).CurrentValues.SetValues(newObject);

      return true; //TODO - Return failure state
    }

    public virtual async Task<int>  Save()
    {
        return await _context.SaveChangesAsync();
    }

    //TODO: Update this to accept T
    public bool RemoveAll(string tableName)
    {
        var result = _context.Database.ExecuteSqlRaw($"DELETE FROM {tableName}");
        return result == 1;
    }
  }
}
