using Moneyman.Domain;
using AutoMapper;

namespace Moneyman.Domain.MapperProfiles
{
    public class TransactionProfile : Profile
    {
        public TransactionProfile()
        {
            CreateMap<Transaction,TransactionDto>()
            .ForMember(a => a.StartDate, b => b.MapFrom(c => c.StartDate))
            .ReverseMap();
        }
    }
}