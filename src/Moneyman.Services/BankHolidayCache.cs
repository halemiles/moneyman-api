using System.Collections.Generic;
using System.Linq;
using Moneyman.Interfaces;

namespace Moneyman.Services
{
    public class BankHolidayCache : IBankHolidayCache
    {
        private IReadOnlyList<string> _holidays = new List<string>();

        public IReadOnlyList<string> Holidays => _holidays;

        public void Populate(IEnumerable<string> holidays)
        {
            _holidays = holidays.ToList();
        }
    }
}
