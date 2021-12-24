using AutoMapper;

namespace Moneyman.Domain.MapperProfiles
{
    public class TransactionToTransactionDtoProfile : Profile
    {
        public TransactionToTransactionDtoProfile()
        {
            CreateMap<Transaction, TransactionDto>();
        }
    }
}