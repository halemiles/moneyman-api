using System.Collections.Generic;
using Moneyman.Domain;
using Moneyman.Interfaces;
using Moneyman.Services.Interfaces;

namespace Moneyman.Services
{
    public class DtpMonthlyGenerator : IDtpGenerator
    {
        public List<PlanDate> Generate(int transactionId)
        {
            throw new System.NotImplementedException();
        }
    }
}