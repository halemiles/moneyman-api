using System;

namespace Moneyman.Domain
{
    public class Transaction : Entity
    {
        
        public string Name { get; set; }
        private decimal amount {get; set;}
        public decimal Amount { 
            get {return Math.Round(amount,2);}
            set {amount = value;}
        }
        public bool Active { get; set; }
        public Frequency Frequency { get; set; }
        public DateTime StartDate { get; set; }
        public bool Anticipated {get; set;}
        //public PaymentType PaymentType { get; set; }
        //public CategoryType CategoryType {get; set;}
        //public PriorityType PriorityType { get; set; }
        //public int UserId {get; set;}  
    }
}
