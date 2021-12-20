using System;
using System.Collections.Generic;
using System.Linq;
using Moneyman.Domain;
using Microsoft.EntityFrameworkCore;

namespace Moneyman.Interfaces
{
    public interface IRepository<T> 
    {
        void Add(T newObject);
        T Get(int id);
        IEnumerable<T> GetAll();
        bool Update(T newObject, int id);
        void Remove(int id);
        int Save();
    }
}
