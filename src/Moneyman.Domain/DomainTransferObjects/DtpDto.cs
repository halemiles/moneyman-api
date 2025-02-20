using System;
using System.Collections.Generic;
using System.Linq;

namespace Moneyman.Models.Dtos
{
    public class DtpDto
    {
        public IEnumerable<PlanDateDto> PlanDates {get; set;}
        public decimal AmountDue {get; set;}
        public decimal SpendPerWeek {get; set;}
        public DateTime StartDate {get; set;}
        public DateTime EndDate {get; set;}
        public int WeeksRemaining {get; set;}
        public decimal Remaining {get; set;}
    }
}