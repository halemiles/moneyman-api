using System;
using FluentValidation;
using Moneyman.Domain;

namespace Moneyman.Services.Validators
{
    public class TransactionValidator :  AbstractValidator<Transaction> //,ITransactionValidator
    {
        public TransactionValidator()
        {
            RuleFor(transaction => transaction.Name).NotNull().NotEmpty();
            RuleFor(transaction => transaction.Amount).GreaterThan(0);
            RuleFor(transaction => transaction.StartDate).GreaterThan(DateTime.MinValue);
        }
    }
}