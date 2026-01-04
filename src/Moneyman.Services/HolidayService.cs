using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Moneyman.Domain.Settings;
using Moneyman.Interfaces;

namespace Moneyman.Services
{
    public class HolidayService : IHolidayService
    {
        private readonly IOptions<HolidayOptions> holidayOptions;
        public HolidayService(IOptions<HolidayOptions> holidayOptions)
        {
            this.holidayOptions = holidayOptions;
        }

        public List<string> GenerateHolidays()
        {
            return holidayOptions.Value.Holidays;
        }
    }
}