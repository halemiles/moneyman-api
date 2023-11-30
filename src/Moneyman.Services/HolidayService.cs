using System.Collections.Generic;
using Moneyman.Interfaces;

namespace Moneyman.Services
{
    public class HolidayService : IHolidayService
    {
        //TODO: Move these to appsettings or another config file
        public List<string> GenerateHolidays()
        {
            return new List<string>
            {
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
                "27-12-2022",

                "02-01-2023",
                "07-04-2023",
                "10-04-2023",
                "01-05-2023",
                "08-05-2023",
                "29-05-2023",                
                "28-08-2023",
                "25-12-2023",
                "26-12-2023",

                "01-01-2024",
                "29-03-2024",
                "01-04-2024",                
                "06-05-2024",
                "27-05-2024",                
                "26-08-2024",
                "25-12-2024",
                "26-12-2024"

            };
        }
    }
}