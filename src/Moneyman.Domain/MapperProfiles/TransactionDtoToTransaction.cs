using AutoMapper;

namespace Moneyman.Domain.MapperProfiles
{
    public class TransactionDtoToTransactionProfile : Profile
    {
        public TransactionDtoToTransactionProfile()
        {
            CreateMap<TransactionDto, Transaction>();
        }
    }
}