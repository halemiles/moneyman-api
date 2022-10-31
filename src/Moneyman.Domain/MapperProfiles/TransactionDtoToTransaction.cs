using System.Diagnostics.CodeAnalysis;
using AutoMapper;

namespace Moneyman.Domain.MapperProfiles
{
    [ExcludeFromCodeCoverage]
    public class TransactionDtoToTransactionProfile : Profile
    {
        public TransactionDtoToTransactionProfile()
        {
            CreateMap<TransactionDto, Transaction>();
        }
    }
}