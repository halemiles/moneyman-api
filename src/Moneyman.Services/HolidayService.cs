using System.Collections.Generic;
using System.Linq;
using Moneyman.Interfaces;

namespace Moneyman.Services
{
    public class HolidayService : IHolidayService
    {
        private readonly IBankHolidayCache _cache;

        public HolidayService(IBankHolidayCache cache)
        {
            _cache = cache;
        }

        public List<string> GenerateHolidays()
        {
            return _cache.Holidays.ToList();
        }
    }
}
