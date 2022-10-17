using System;

namespace Moneyman.Domain
{
    public class DteObject
    {
        public DateTime PlanDate { get; set; }
        public DateTime OriginalPlanDate { get; set; }
        public string PlanDateString { get { return PlanDate.ToString("dd-MM-YYYY");} }
        //public string OriginalPlanDateString { get { return OriginalPlanDate.ToString("dd-MM-YYYY");}  }
        public bool IsBankHoliday { get; set; }
        public bool IsValid { get; set; }
        public int OffsetBy { get; set; }
        public string Reason { get; set; }
        
    }
}