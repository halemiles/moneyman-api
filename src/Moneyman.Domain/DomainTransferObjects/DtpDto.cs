using System;
using System.Collections.Generic;
using System.Linq;

namespace Moneyman.Models.Dtos
{
    public class DtpDto
    {
        public IEnumerable<PlanDateDto> PlanDates {get; set;}
        public decimal AmountDue => Math.Abs(PlanDates.Sum(x => x.Amount));
        public DateTime StartDate {get; set;}
        public DateTime EndDate {get; set;}
    }
}