using Moneyman.Domain;
using Moneyman.Interfaces;

namespace Moneyman.Persistence
{
    public class BankHolidayRepository : GenericRepository<BankHoliday>, IBankHolidayRepository
    {
        public BankHolidayRepository(MoneymanContext context) : base(context) { }
    }
}
