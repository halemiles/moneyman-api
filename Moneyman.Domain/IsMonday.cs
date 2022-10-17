using System;
using System.Collections.Generic;

namespace Moneyman.Extensions
{
    public static class DateVerificationExtensions
    {
        public static bool IsMonday(this DateTime dte)
        {
            return dte.DayOfWeek == DayOfWeek.Monday;
        }

        public static bool IsWeekday(this DateTime dte)
        {
            return dte.DayOfWeek != DayOfWeek.Saturday && dte.DayOfWeek != DayOfWeek.Sunday;
        }

        public static bool IsBankHoliday(this List<string> holidays, DateTime dte)
        {
            return holidays.Contains(dte.ToString("dd-MM-yyyy"));
        }
    }
}