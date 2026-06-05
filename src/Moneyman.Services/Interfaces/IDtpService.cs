using System.Collections.Generic;
using System.Threading.Tasks;
using Moneyman.Domain;
using Moneyman.Domain.Models;
using Moneyman.Models.DomainTransferObjects;

namespace Moneyman.Services.Interfaces
{
    public interface IDtpService
    {
        Task<ApiResponse<List<PlanDate>>> GenerateAll(int? transactionId);
        ApiResponse<DtpDto> GetCurrent(int? startingValue);
        ApiResponse<DtpDto> GetOffset(int? monthOffset);
    }
}
