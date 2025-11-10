using System;
using System.Collections.Generic;
using System.Linq;

namespace Moneyman.Models.DomainTransferObjects
{
    public class DtpDto
    {
        public IEnumerable<PlanDateDto> PlanDates {get; set;}
        public decimal AmountDue {get; set;} //=> Math.Abs(PlanDates.Sum(x => x.Amount));
        public decimal SpendPerWeek {get; set;} // => Math.Abs(PlanDates.Sum(x => x.Amount) / WeeksRemaining);
        public DateTime StartDate {get; set;}
        public DateTime EndDate {get; set;}
        public int WeeksRemaining {get; set;}
        public decimal Remaining {get; set;}
    }
}