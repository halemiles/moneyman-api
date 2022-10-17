using System.Collections.Generic;
using Moneyman.Domain;

namespace Moneyman.Services.Interfaces
{
    public interface IDtpService
    {
        List<PlanDate> GenerateAll();

        List<PlanDate> GenerateForTransaction(int transactionId);

        List<PlanDate> GenerateMonthly(int transactionId);

        List<PlanDate> GenerateWeekly(int transactionId);

        List<PlanDate> GenerateDaily(int transactionId);

        List<PlanDate> GenerateYearly(int transactionId);
    }
}