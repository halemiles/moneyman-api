using Moneyman.Domain;
using AutoMapper;
using Moneyman.Models.Dtos;

namespace Moneyman.Domain.MapperProfiles
{
    public class PlanDateDtoProfile : Profile
    {
        public PlanDateDtoProfile()
        {
            CreateMap<PlanDate,PlanDateDto>()
            .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.Date))
            .ForMember(dest => dest.TransactionName, opt => opt.MapFrom(src => src.Transaction.Name))
            .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.Transaction.Amount))
            .ReverseMap();
        }
    }
}