
using Moneyman.Interfaces;
using System.Collections.Generic;
using System.Linq;
using Moneyman.Domain;
using AutoMapper;

namespace Moneyman.Persistence
{
    public class PaydayRepository : GenericRepository<Payday>, IPaydayRepository
    {
        public PaydayRepository(MoneymanContext context, IMapper mapper) : base(context, mapper)
        {
        }
    }
}
