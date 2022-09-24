using System;

namespace Moneyman.Domain
{   
    //public enum Frequency {SINGLE, DAY, WEEK, MONTH, YEAR, MONTHLASTDAY};
    public enum PaymentType {MANUAL, STANDINGORDER, DIRECTDEBIT};
    public enum CategoryType {OTHER, HOUSING, TRANSPORTATION, PERSONALCARE, ENTERTAINMENT, TAXES, INSURANCE, LOANS, SAVINGS, FOOD, SERVICES};
    public enum PriorityType {DESIRED, DEPENDANT, ESSENTIAL};

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
        public PaymentType PaymentType { get; set; }
        public CategoryType CategoryType {get; set;}
        public PriorityType PriorityType { get; set; }
        public int UserId {get; set;}  
    }
}
