
using Api.Interfaces;
using Api.Models;
using System.Collections.Generic;
using System.Linq;
using Moneyman.Domain;

namespace Api.Data
{
    public class TransactionRepository : GenericRepository<Transaction>, ITransactionRepository
    {
        public TransactionRepository(MoneymanContext context) : base(context)        
        {
        }

        
    }
}
