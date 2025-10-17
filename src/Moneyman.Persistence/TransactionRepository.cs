
using Moneyman.Interfaces;
using System.Collections.Generic;
using System.Linq;
using Moneyman.Domain;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Moneyman.Persistence
{
    public class TransactionRepository : GenericRepository<Transaction>, ITransactionRepository
    {
        public TransactionRepository(MoneymanContext context, IMapper mapper) : base(context, mapper)
        {
        }

        public override void Remove(int id)
        {
            var planDates = _context.PlanDates.Where(p => p.Transaction.Id == id);
            _context.RemoveRange(planDates);
            base.Remove(id);
        }

        public override bool Update(Transaction newObject)
        {

            _context.Update(newObject);
            int recordCount = _context.SaveChanges();
            return recordCount > 0;
        }

    }
}
