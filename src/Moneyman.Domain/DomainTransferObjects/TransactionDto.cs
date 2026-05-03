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
        public PaymentType? PaymentType { get; set; }
        public int? BankAccountId { get; set; }

        public string PaymentTypeDesc => PaymentType.HasValue ? PaymentType.Value.ToString() : null;
        public CategoryType? CategoryType {get; set;}
        public string CategoryTypeDesc => CategoryType.HasValue ? CategoryType.Value.ToString() : null;
        public PriorityType? PriorityType { get; set; }

        public string PriorityTypeDesc => PriorityType.HasValue ? PriorityType.Value.ToString() : null;
    }
}