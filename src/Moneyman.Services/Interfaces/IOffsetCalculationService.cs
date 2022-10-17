using System;
using Moneyman.Domain;

namespace Moneyman.Interfaces
{
    public interface IOffsetCalculationService
    {
        DteObject CalculateOffset(DateTime dte); //TODO - Should we just return a date?
    }
}