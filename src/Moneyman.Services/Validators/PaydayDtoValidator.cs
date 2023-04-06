using System;
using FluentValidation;
using Moneyman.Domain;
using Moneyman.Models;

namespace Moneyman.Services.Validators
{
    public class PaydayDtoValidator :  AbstractValidator<PaydayDto>
    {
        public PaydayDtoValidator()
        {
            RuleFor(payday => payday.DayOfMonth).NotNull().NotEmpty();
        }
    }
}