using Moneyman.Domain;
using AutoMapper;
using Moneyman.Domain.Models;
using Moneyman.Models.Dtos;

namespace Moneyman.Domain.MapperProfiles
{
    public class DtpDtoProfile : Profile
    {
        public DtpDtoProfile()
        {
            CreateMap<Dtp,DtpDto>()
            .ForAllMembers(o => o.Condition((_, _, member) => member != null));
        }
    }
}