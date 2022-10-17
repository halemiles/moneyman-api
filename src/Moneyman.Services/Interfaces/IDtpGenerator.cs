using System.Collections.Generic;
using Moneyman.Domain;

namespace Moneyman.Services
{
    public interface IDtpGenerator
    {
        List<PlanDate> Generate(int transactionId);
        
        
    }
}