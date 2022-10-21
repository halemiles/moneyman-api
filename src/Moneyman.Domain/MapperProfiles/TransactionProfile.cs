using Moneyman.Domain;
using AutoMapper;

namespace Moneyman.Domain.MapperProfiles
{
    public class TransactionProfile : Profile
    {
        public TransactionProfile()
        {
            CreateMap<Transaction,Transaction>()
            .ForAllMembers(o => o.Condition((_, destination, member) => member != null));
        }
    }
}