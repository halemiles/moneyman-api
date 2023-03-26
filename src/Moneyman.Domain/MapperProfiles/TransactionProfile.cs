using Moneyman.Domain;
using AutoMapper;

namespace Moneyman.Domain.MapperProfiles
{
    public class TransactionProfile : Profile
    {
        public TransactionProfile()
        {
            CreateMap<Transaction,TransactionDto>()
            .ForAllMembers(o => o.Condition((_, _, member) => member != null));
        }
    }
}