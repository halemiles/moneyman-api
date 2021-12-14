using System.Collections.Generic;
using Api.Models;
using Moneyman.Domain;

namespace Api.Interfaces
{
    public interface ITransactionRepository : IRepository<Transaction>
    { 
    }
}
