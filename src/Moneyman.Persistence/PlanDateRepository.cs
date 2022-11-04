
using Moneyman.Interfaces;
using System.Collections.Generic;
using System.Linq;
using Moneyman.Domain;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace Moneyman.Persistence
{
    public class PlanDateRepository : GenericRepository<PlanDate>, IPlanDateRepository
    {
        public PlanDateRepository(MoneymanContext context, IMapper mapper) : base(context, mapper)
        {
        }

        public override IEnumerable<PlanDate> GetAll()
        {

            return  _context.PlanDates.Include(x => x.Transaction).AsEnumerable();
        }
        
    }
}
