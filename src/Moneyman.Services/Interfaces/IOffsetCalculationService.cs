using System;
using Moneyman.Domain;

namespace Moneyman.Interfaces
{
    public interface IOffsetCalculationService
    {
        CalculatedPlanDate CalculateOffset(DateTime dte);
    }
}