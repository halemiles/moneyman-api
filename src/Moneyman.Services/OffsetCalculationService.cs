using System;
using Moneyman.Interfaces;
using Moneyman.Domain;
using Moneyman.Extensions;

namespace Moneyman.Services
{
    public class OffsetCalculationService : IOffsetCalculationService
    {
        private readonly IWeekdayService _weekdayService;
        private readonly IHolidayService _holidayService;
        public OffsetCalculationService(
            IWeekdayService weekdayService,
            IHolidayService holidayService
        )
        {
            _weekdayService = weekdayService;
            _holidayService = holidayService;
        }
        
        
        public CalculatedPlanDate CalculateOffset(DateTime dte)
        {
            var holidays = _holidayService.GenerateHolidays();
            var returnObject = new CalculatedPlanDate
            {
                OriginalPlanDate = dte
            };        

            int offsetby = 0;
            bool found = false;
            int foundLoopCount = 0;

            if(dte.Month == 12)
                dte = dte.AddYears(1);

            //Iterate until we have found a suitable date
            while (!found)
            {
                //Check if this current iteration is on a weekend, monday or a bank holiday
                bool isWeekday = dte.IsWeekday();
                bool isBankHoliday = holidays.IsBankHoliday(dte);
                
                //If we have found a valid day (Tue-Fri and not on bank holiday)
                if(isWeekday && !isBankHoliday )
                {
                    
                    returnObject.PlanDate = dte;
                    returnObject.IsBankHoliday = isBankHoliday;
                    returnObject.IsValid = true;
                    found = true;
                }
                else
                {
                    //Move forward by 1 day
                    //Note: Direct debits typically come out Mondays or Tuesdays
                    //      if they fall on a weekend or a bank holiday
                    dte = dte.AddDays(1);
                    offsetby += 1;

                    returnObject.OffsetBy = offsetby;
                    returnObject.Reason = "On bank holiday or weekend";  
                    returnObject.PlanDate = dte;
                    
                }

                //This is to prevent infinate loops
                //TODO - Unit test to make sure this doesnt happen in the first place
                if(foundLoopCount >=10)
                {
                    found  = true;
                }
                                
                foundLoopCount ++;
                
            }
            return returnObject;
        } 
    }
}