using System;
using Moneyman.Interfaces;

namespace Moneyman.Services
{
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTime GetNow()
        {
            return DateTime.Now;
        }

        public DateTime GetToday()
        {
            return DateTime.Today;
        }
    }
}