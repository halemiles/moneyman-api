using System;
using Moneyman.Domain;
using Moneyman.Domain.Models;
using Moneyman.Models.DomainTransferObjects;

namespace Moneyman.Interfaces
{
    public interface IOffsetCalculationService
    {
        CalculatedPlanDate CalculateOffset(DateTime dte);
        ApiResponse<DtpDto> GetPlanDatesByPeriod(int? monthOffset);
    }
}