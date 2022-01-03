using System;
using System.Collections.Generic;
using System.Linq;
using Moneyman.Domain;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Moneyman.Interfaces
{
    public interface IRepository<T> 
    {
        void Add(T newObject);
        T Get(int id);
        IEnumerable<T> GetAll();
        bool Update(T newObject);
        void Remove(int id);
        Task<int> Save();
    }
}
