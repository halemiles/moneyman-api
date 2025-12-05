using System.ComponentModel.DataAnnotations;

namespace Moneyman.Services
{
    public class PaydayOptions
    {
        /// <summary>
        /// The day of the month when payday occurs. Must be between 1 and 31.
        /// </summary>
        [Range(1, 31)]
        public int DayOfMonth { get; set; }
    }
}
