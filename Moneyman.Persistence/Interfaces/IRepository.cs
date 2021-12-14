using System;
using System.Collections.Generic;
using System.Linq;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Interfaces
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
