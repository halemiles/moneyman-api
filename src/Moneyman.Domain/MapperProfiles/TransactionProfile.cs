using Moneyman.Domain;
using AutoMapper;

namespace Moneyman.Domain.MapperProfiles
{
    public class TransactionDtoProfile : Profile
    {
        public TransactionDtoProfile()
        {
            CreateMap<Transaction,TransactionDto>()
            .ForMember(a => a.StartDate, b => b.MapFrom(c => c.StartDate))
            .ReverseMap();
        }
    }

    public class TransactionProfile : Profile
    {
        public TransactionProfile()
        {
            CreateMap<Transaction, Transaction>()
                .ReverseMap()
                .ForAllMembers(opts => opts.Condition((src, dest, srcMember) =>
                    srcMember != null &&
                    (!(srcMember is string) || !string.IsNullOrEmpty((string)srcMember))));
        }
    }
}