using System.Collections.Generic;
using Moneyman.Domain;
using Moneyman.Domain.Models;
using Moneyman.Models.Dtos;

namespace Moneyman.Services.Interfaces
{
    public interface IDtpService
    {
        ApiResponse<List<PlanDate>> GenerateAll(int? transactionId);

        List<PlanDate> GenerateForTransaction(int? transactionId);

        List<PlanDate> GenerateMonthly(int? transactionId);

        List<PlanDate> GenerateWeekly(int? transactionId);

        List<PlanDate> GenerateDaily(int? transactionId);

        List<PlanDate> GenerateYearly(int? transactionId);
        ApiResponse<DtpDto> GetCurrent(int? startingValue, int? accountId);
        DtpDto GetOffset(int? monthOffset);

    }
}