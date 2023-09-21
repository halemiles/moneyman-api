using System;
using System.Diagnostics.CodeAnalysis;

namespace Moneyman.Domain
{
    public class TransactionDto
    {
        public int Id {get; set;}
        public string Name { get; set; }
        public decimal? Amount {get; set;}
        public DateTime? StartDate {get; set;}
        public bool? Active { get; set; }
        public Frequency? Frequency { get; set; }
        public bool? IsAnticipated {get; set;}
        public PaymentType? PaymentType { get; set; }

        public string PaymentTypeDesc => PaymentType.ToString();
        public CategoryType? CategoryType {get; set;}
        public string CategoryTypeDesc => CategoryType.ToString();
        public PriorityType? PriorityType { get; set; }     

        public string PriorityTypeDesc => PriorityType.ToString();
    }
}