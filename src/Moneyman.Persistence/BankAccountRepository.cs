
using Moneyman.Interfaces;
using System.Collections.Generic;
using System.Linq;
using Moneyman.Domain;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Moneyman.Persistence
{
    public class BankAccountRepository : GenericRepository<BankAccount>, IBankAccountRepository
    {
        public BankAccountRepository(MoneymanContext context, IMapper mapper) : base(context, mapper)
        {
        }
    }
}
