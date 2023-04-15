using System;
using FluentValidation;
using Moneyman.Domain;

namespace Moneyman.Services.Validators
{
    public class TransactionDtoValidator :  AbstractValidator<TransactionDto> //TODO: Introduce an IValiator so we can do some DI
    {
        public TransactionDtoValidator()
        {
            RuleFor(transaction => transaction.Name).NotNull().NotEmpty();
            RuleFor(transaction => transaction.Amount).NotNull().GreaterThan(0);
            RuleFor(transaction => transaction.Date).GreaterThan(DateTime.MinValue);
        }
    }
}