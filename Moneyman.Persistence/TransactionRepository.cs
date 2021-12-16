
using Moneyman.Interfaces;
using Moneyman.Models;
using System.Collections.Generic;
using System.Linq;
using Moneyman.Domain;

namespace Moneyman.Persistence
{
    public class TransactionRepository : GenericRepository<Transaction>, ITransactionRepository
    {
        public TransactionRepository(MoneymanContext context) : base(context)        
        {
        }

        
    }
}
