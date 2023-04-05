using System;
using FluentValidation;
using Moneyman.Domain;

namespace Moneyman.Services.Validators
{
    public class TransactionValidator :  AbstractValidator<TransactionDto> //,ITransactionValidator
    {
        public TransactionValidator()
        {
            RuleFor(transaction => transaction.Name).NotNull().NotEmpty();
            RuleFor(transaction => transaction.Amount).GreaterThan(0);
            RuleFor(transaction => transaction.Date).GreaterThan(DateTime.MinValue);
        }
    }
}