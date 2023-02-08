using System;
using Moneyman.Domain;

namespace Moneyman.Domain
{
    public class BaseTransaction : Entity
    {
        public string Name {get; set;}
        private decimal amount {get; set;}
        public decimal Amount { 
            get {return Math.Round(amount,2);}
            set {amount = value;}
        }

        public bool Active { get; set; }

        public DateTime StartDate { get; set; }
    }
}