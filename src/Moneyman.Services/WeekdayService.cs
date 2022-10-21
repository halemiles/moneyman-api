using System.Collections.Generic;
using Moneyman.Domain;
using Moneyman.Interfaces;

namespace Moneyman.Services
{
    public class WeekdayService : IWeekdayService
    {
        public List<WeekDay> GenerateWeekdays()
        {
            return new List<WeekDay>
            {
           
                new WeekDay {Name="Monday", OffsetVal = 0,Reason=""},
                new WeekDay {Name="Tuesday",OffsetVal=0,Reason=""},
                new WeekDay {Name="Wednesday",OffsetVal=0,Reason=""},
                new WeekDay {Name="Thursday",OffsetVal=0,Reason=""}, 
                new WeekDay {Name="Friday",OffsetVal=0,Reason=""},
                new WeekDay {Name="Saturday",OffsetVal=-1,Reason="saturday"},
                new WeekDay {Name="Sunday",OffsetVal=-2,Reason="sunday"}
            };
        }
    }
}