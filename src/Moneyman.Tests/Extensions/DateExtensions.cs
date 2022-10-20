using System;

namespace Moneyman.Tests.Extensions
{
    public static class DateExtensions
    {
        public static DateTime WithDate(this DateTime dte, int dayOfMonth)
        {
            return new DateTime(dte.Year, dte.Month, dayOfMonth);
        }

        public static DateTime WithMonth(this DateTime dte, int month)
        {
            return new DateTime(dte.Year, month, dte.Day);
        }
    }
}