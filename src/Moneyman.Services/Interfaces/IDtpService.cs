using System.Collections.Generic;
using Moneyman.Domain;
using Moneyman.Domain.Models;

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
    }
}