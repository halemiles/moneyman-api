
using Moneyman.Interfaces;
using System.Collections.Generic;
using System.Linq;
using Moneyman.Domain;
using AutoMapper;

namespace Moneyman.Persistence
{
    public class PlanDateRepository : GenericRepository<PlanDate>, IPlanDateRepository
    {
        public PlanDateRepository(MoneymanContext context, IMapper mapper) : base(context, mapper)
        {
        }

        
    }
}
