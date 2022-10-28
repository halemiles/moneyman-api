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
                YearGroup = 1, //TODO - Needs actual data
                MonthGroup = 1, //TODO - Needs actual data
                IsAnticipated = false, //TODO - Needs actual data
                OrderId = 0, //TODO - Needs actual data
                Transaction = transaction
            };
        }
    }
}