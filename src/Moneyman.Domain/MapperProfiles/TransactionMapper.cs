using System.Collections.Generic;
using System.Linq;
using Riok.Mapperly.Abstractions;
using Moneyman.Domain;

namespace Moneyman.Domain.MapperProfiles
{
    [Mapper]
    public partial class TransactionMapper
    {
        public partial TransactionDto ToDto(Transaction entity);
        public partial Transaction ToEntity(TransactionDto dto);

        public List<Transaction> ToEntityList(IEnumerable<TransactionDto> dtos)
        {
            return dtos.Select(ToEntity).ToList();
        }
    }
}
