using System.ComponentModel.DataAnnotations;

namespace Moneyman.Services
{
    public class BankHolidayApiOptions
    {
        public const string DefaultUrl = "https://www.gov.uk/bank-holidays.json";
        public const string DefaultRegion = "england-and-wales";

        [Required]
        public string Url { get; set; } = DefaultUrl;

        [Required]
        public string Region { get; set; } = DefaultRegion;
    }
}
