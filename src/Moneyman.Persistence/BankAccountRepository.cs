
using Moneyman.Interfaces;
using Moneyman.Domain;
using AutoMapper;

namespace Moneyman.Persistence
{
    public class BankAccountRepository : GenericRepository<BankAccount>, IBankAccountRepository
    {
        public BankAccountRepository(MoneymanContext context, IMapper mapper) : base(context, mapper)
        {
        }
    }
}
