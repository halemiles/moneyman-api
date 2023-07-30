
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
            _mapper.Map(newObject, existing);
                
            if (existing == null)
            {
                _context.Add(newObject);
                return true;
            }

            _context.Update(newObject);
            _context.SaveChanges();
            return true; //TODO - Return failure state
        }
    }
}
