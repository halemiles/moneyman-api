using System.Collections.Generic;

namespace Moneyman.Models.Dtos
{
    public class DtpDto
    {
        IEnumerable<PlanDateDto> PlanDates {get; set;}
    }
}