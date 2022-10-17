using System;

namespace Moneyman.Domain
{
    public class TransactionDto
    {
        public int Id {get; set;}
        public string Name { get; set; }
        public decimal Amount {get; set;}
        public DateTime Date {get; set;}
        public bool Active { get; set; }
        public Frequency Frequency { get; set; }
    }
}