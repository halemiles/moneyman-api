
using Moneyman.Interfaces;
using Moneyman.Domain;

namespace Moneyman.Persistence
{
    public class PaydayRepository : GenericRepository<Payday>, IPaydayRepository
    {
        public PaydayRepository(MoneymanContext context) : base(context)
        {
        }
    }
}
