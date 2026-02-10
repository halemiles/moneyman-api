using System;

namespace Moneyman.Models.DomainTransferObjects
{
    public class PlanDateDto
    {
        public string TransactionName {get; set;}
        public decimal Amount {get; set;}
        public DateTime Date {get; set;}

        public int BankAccountId {get; set;}

        // Expose the originating transaction UniqueId so clients can correlate
        public string TransactionUniqueId { get; set; }
    }
}