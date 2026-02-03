using System;
using Moneyman.Domain;

namespace Moneyman.Services.Factories
{
    public class PlanDateFactory
    {
        private readonly Transaction transaction;
        private readonly DateTime calculatedOffsetDate;
        public PlanDateFactory(Transaction transaction, DateTime calculatedOffsetDate )
        {
            this.transaction = transaction;
            this.calculatedOffsetDate = calculatedOffsetDate;
        }

        public PlanDate Create()
        {
            return new PlanDate
            {
                Active = true,
                Date = calculatedOffsetDate,
                OriginalDate = transaction.StartDate,
                Transaction = transaction
            };
        }
    }
}