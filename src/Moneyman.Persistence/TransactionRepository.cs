
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

        public override bool Update(Transaction newObject)
        {
            IEntity entity = (IEntity)newObject;
            var existing = _context.Set<Transaction>().AsNoTracking().FirstOrDefault(x => x.Id == entity.Id);

            if (existing == null)
            {
                return false;
            }

            _mapper.Map(newObject, existing);

            //TODO - Update this in the mapping profile
            newObject.Name = existing.Name;

            if(newObject.StartDate == System.DateTime.MinValue)
            {
                newObject.StartDate = existing.StartDate;
            }

            _context.Update(newObject);
            int recordCount = _context.SaveChanges();
            return recordCount > 0;
        }
        
    }
}
