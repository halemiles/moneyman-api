
using Moneyman.Interfaces;
using System.Linq;
using Moneyman.Domain;

namespace Moneyman.Persistence
{
    public class TransactionRepository : GenericRepository<Transaction>, ITransactionRepository
    {
        public TransactionRepository(MoneymanContext context) : base(context)
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
