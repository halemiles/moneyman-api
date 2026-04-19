using System;

namespace Moneyman.Domain
{
    public class BankHoliday : Entity
    {
        public DateTime Date { get; set; }
        public string Title { get; set; }
    }
}
