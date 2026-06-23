using System.ComponentModel.DataAnnotations;

namespace Moneyman.Services
{
    public class BankHolidayApiOptions
    {
        // Sensible default for the UK gov bank-holidays feed; overridable via BankHolidayApiOptions:Url in config.
#pragma warning disable S1075
        public const string DefaultUrl = "https://www.gov.uk/bank-holidays.json";
#pragma warning restore S1075
        public const string DefaultRegion = "england-and-wales";

        [Required]
        public string Url { get; set; } = DefaultUrl;

        [Required]
        public string Region { get; set; } = DefaultRegion;
    }
}
