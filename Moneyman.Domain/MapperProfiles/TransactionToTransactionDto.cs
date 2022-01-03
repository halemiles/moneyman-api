using System.Diagnostics.CodeAnalysis;
using AutoMapper;

namespace Moneyman.Domain.MapperProfiles
{
    [ExcludeFromCodeCoverage]
    public class TransactionToTransactionDtoProfile : Profile
    {
        public TransactionToTransactionDtoProfile()
        {
            CreateMap<Transaction, TransactionDto>();
        }
    }
}