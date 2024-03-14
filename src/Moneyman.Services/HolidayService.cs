using System.Collections.Generic;
using Microsoft.Extensions.Options;
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


        //TODO: Move these to appsettings or another config file
        public List<string> GenerateHolidays()
        {
            return holidayOptions.Value.Holidays;
        }
    }

    public class HolidayOptions
    {
        public List<string> Holidays {get; set;}
    }
}