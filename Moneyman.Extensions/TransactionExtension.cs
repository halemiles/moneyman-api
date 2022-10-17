using System;
using Api.Models;

namespace Moneyman.Extensions
{
    public class TransactionHelper
    {
        public MoneyTransaction GenerateSingleTransaction(
            int? id = 0,
            string name = "Test Transaction",
            DateTime? start = null,
            decimal? amount = 0)
        {
            return new MoneyTransaction(){
                    Id=id.Value,
                    Name=name,
                    Active = true,
                    StartDate = start.Value,
                    Frequency= TransactionFrequency.MONTH,
                    Amount = amount.Value,
                    PaymentType = PaymentType.DIRECTDEBIT,
                    CategoryType = CategoryType.ENTERTAINMENT,
                    PriorityType = PriorityType.DEPENDANT,
                    UserId=0
                };
        }

        public void SetDefaultData(MoneyTransaction trans)
        {
            //TODO - We can to this with => operator now.
            trans.Active = true;
            trans.PaymentType = PaymentType.DIRECTDEBIT;
            trans.CategoryType = CategoryType.ENTERTAINMENT;
            trans.PriorityType = PriorityType.DEPENDANT;
            trans.UserId=0;
        }
    }
}