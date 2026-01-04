using System;
using Moneyman.Domain;

namespace Moneyman.Domain.Interfaces
{
    public class BaseTransaction : Entity
    {
        public string Name {get; set;}
        public decimal Amount {get; set;}
        public bool Active { get; set; }

        public DateTime StartDate { get; set; }

        public int BankAccountId { get; set; }
    }
}