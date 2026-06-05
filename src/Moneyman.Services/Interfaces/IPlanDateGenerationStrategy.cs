using System.Collections.Generic;
using Moneyman.Domain;

namespace Moneyman.Interfaces
{
    public interface IPlanDateGenerationStrategy
    {
        List<PlanDate> Generate(int? transactionId, Frequency frequency);
    }
}
