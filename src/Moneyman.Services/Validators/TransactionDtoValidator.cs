using System;
using FluentValidation;
using Moneyman.Domain;

namespace Moneyman.Services.Validators
{
    public class TransactionDtoValidator :  AbstractValidator<TransactionDto>
    {
        public TransactionDtoValidator()
        {
            RuleFor(transaction => transaction.Name).NotNull().NotEmpty();
            RuleFor(transaction => transaction.Amount).GreaterThan(0);
            RuleFor(transaction => transaction.Date).GreaterThan(DateTime.MinValue);
        }
    }
}