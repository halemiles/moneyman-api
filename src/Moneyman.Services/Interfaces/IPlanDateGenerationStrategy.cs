using System.Collections.Generic;
using Moneyman.Domain;

namespace Moneyman.Interfaces
{
    public interface IPlanDateGenerationStrategy
    {
        public List<PlanDate> Generate(int? transactionId, Frequency frequency);
    }
}