using System;

namespace Moneyman.Domain
{
    public class Transaction : BaseTransaction
    {
        public Frequency Frequency { get; set; }
        public bool IsAnticipated {get; set;}
        
        public PaymentType PaymentType { get; set; }
        public CategoryType CategoryType {get; set;}
        public PriorityType PriorityType { get; set; }        
    }

    public class AnticipatedTransaction : BaseTransaction
    {
    }
}
