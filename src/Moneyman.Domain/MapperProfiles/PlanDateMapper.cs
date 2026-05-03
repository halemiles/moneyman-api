using System.Collections.Generic;
using System.Linq;
using Moneyman.Models.DomainTransferObjects;

namespace Moneyman.Domain.MapperProfiles
{
    public class PlanDateMapper
    {
        public PlanDateDto ToDto(PlanDate entity)
        {
            if (entity == null || entity.Transaction == null)
                return null;
            return new PlanDateDto
            {
                TransactionName = entity.Transaction.Name,
                Amount = entity.Transaction.Amount,
                BankAccountId = entity.Transaction.BankAccountId ?? 0,
                Date = entity.Date
            };
        }

        public PlanDate ToEntity(PlanDateDto dto)
        {
            if (dto == null)
                return null;
            return new PlanDate
            {
                Date = dto.Date,
                Transaction = new Transaction
                {
                    Name = dto.TransactionName,
                    Amount = dto.Amount,
                    BankAccountId = dto.BankAccountId
                }
            };
        }

        public List<PlanDateDto> ToDtoList(IEnumerable<PlanDate> entities)
        {
            return entities.Select(ToDto).ToList();
        }
    }
}
