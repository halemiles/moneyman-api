using System;
using System.Collections.Generic;
using System.Linq;
using Moneyman.Interfaces;
using Microsoft.Extensions.Logging;
using Moneyman.Domain;
using Moneyman.Extensions;

namespace Moneyman.Services
{
    public class OffsetCalculationService : IOffsetCalculationService
    {
        private readonly IWeekdayService _weekdayService;
        public OffsetCalculationService(
            IWeekdayService weekdayService
        )
        {
            _weekdayService = weekdayService;
        }
        public List<string> GenerateHolidays()
        {
            return new List<string>()
            {
                "01-01-2019",
                "19-04-2019",
                "22-04-2019",
                "06-05-2019",
                "27-05-2019",
                "26-08-2019",
                "25-12-2019",
                "26-12-2019",
                "01-01-2020",
                "10-04-2020",
                "13-04-2020",
                "04-05-2020",
                "25-05-2020",
                "31-08-2020",
                "25-12-2020",
                "26-12-2020",
                "28-12-2020",
                

                "01-01-2021",
                "02-04-2021",
                "05-04-2021",
                "03-05-2021",
                "31-05-2021",
                "30-08-2021",
                "27-12-2021",
                "28-12-2021",

                "03-01-2022",
                "15-04-2022",
                "18-04-2022",
                "02-05-2022",
                "02-06-2022",
                "03-06-2022",
                "29-08-2022",
                "26-12-2022",
                "27-12-2022"

            };
        }
        
        public DteObject CalculateOffset(DateTime dte)
        {
            var weekDays = _weekdayService.GenerateWeekdays();
            var holidays = GenerateHolidays();
            DateTime originalDate = dte;
            var returnObject = new DteObject();

            WeekDay offset = weekDays[(int)dte.DayOfWeek];           

            int offsetby = 0;
            bool found = false;
            int foundLoopCount = 0;

            while (!found)
            {
                bool isWeekday = dte.IsWeekday();
                bool isBankHoliday = holidays.IsBankHoliday(dte);
                bool isMonday = dte.IsMonday();
                
                if(isWeekday && !isBankHoliday )
                {
                    
                    returnObject.PlanDate = dte;
                    returnObject.IsBankHoliday = isBankHoliday;
                    returnObject.IsValid = true;
                    found = true;
                }
                else
                {
                    
                    dte = dte.AddDays(1);
                    offsetby += 1;
                    offset = weekDays[(int)dte.DayOfWeek];
                    isWeekday = dte.IsWeekday();
                    isBankHoliday = holidays.IsBankHoliday(dte);

                    returnObject.OffsetBy = offsetby;
                    returnObject.Reason = "On bank holiday or weekend";  
                    returnObject.PlanDate = dte; 
                    
                }
                if(foundLoopCount >=10)
                {
                    found  = true;
                    //("Hit Limit");
                }
                //Console.WriteLine($"{dte.ToString("m")} we:{isWeekday} bh:{isBankHoliday}");              
                foundLoopCount ++;
                //Console.WriteLine($"{dte.ToLongDateString()}: {returnObject.PlanDate.ToLongDateString()}");
            }
            return returnObject;
        }

        public DateTime GenerateDayOfMonth(DateTime dte, int mon, int year)
        {
            return DateTime.Now;
        }

        public DateTime LastWorkdayOfMonth(int mon, int year)
        {
            //DateTime start = new DateTime(year, mon, 1);
            DateTime start = new DateTime(year, mon, DateTime.DaysInMonth(year, mon));
            int offset = 0;
            if(start.DayOfWeek == DayOfWeek.Sunday) //Sunday
            {
                offset = -2;
            }

            if(start.DayOfWeek == DayOfWeek.Saturday) //Saturday
            {  
                offset = -1; 
            }
            
            return start.AddDays(offset);
        }
    }
}