using System;

namespace Moneyman.Domain
{
    public class Transaction : Entity
    {
        public string Name { get; set; }
        public decimal Amount {get; set;}
        public DateTime Date {get; set;}
        public bool Active { get; set; }
        public Frequency Frequency { get; set; }
    }
}