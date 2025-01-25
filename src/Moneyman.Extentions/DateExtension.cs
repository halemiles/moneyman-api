using System;

namespace Moneyman.Extensions
{
    public static class DateHelper
    {
        public static DateTime EndOfMonth(DateTime dte)
        {
           return new DateTime(dte.Year, 
                                   dte.Month, 
                                   DateTime.DaysInMonth(dte.Year, 
                                                        dte.Month));
        }
    }
}