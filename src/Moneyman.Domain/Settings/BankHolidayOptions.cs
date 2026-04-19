namespace Moneyman.Domain.Settings
{
    public class BankHolidayOptions
    {
        public string Url { get; set; } = "https://www.gov.uk/bank-holidays.json";
        public string Region { get; set; } = "england-and-wales";
    }
}
