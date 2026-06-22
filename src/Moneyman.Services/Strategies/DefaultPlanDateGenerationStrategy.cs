using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moneyman.Domain;
using Moneyman.Interfaces;
using Moneyman.Services.Extentions;

namespace Moneyman.Services
{
    public class DefaultPlanDateGenerationStrategy : IPlanDateGenerationStrategy
    {
        private readonly ITransactionRepository transactionRepository;
        private readonly IOffsetCalculationService offsetCalculationService;
        private readonly ILogger<DtpService> logger;
        private const int TotalPlanDateYears = 2;

        public DefaultPlanDateGenerationStrategy(
            ITransactionRepository transactionRepository,
            IOffsetCalculationService offsetCalculationService,
            ILogger<DtpService> logger
        )
        {
            this.transactionRepository = transactionRepository;
            this.offsetCalculationService = offsetCalculationService;
            this.logger = logger;
        }

        public List<PlanDate> Generate(int? transactionId, Frequency frequency)
        {
            logger.LogInformation("Generating {Frequency} plan dates", frequency);

            var transactions = transactionRepository.GetAll()
                .Where(x => x.Frequency == frequency);
            if (transactionId.HasValue)
            {
                transactions = transactions.Where(x => x.Id == transactionId);
            }

            List<PlanDate> planDates = new();
            int totalCount = frequency == Frequency.Anticipated
                ? 1
                : frequency.ToFrequencyCount() * TotalPlanDateYears;

            foreach (var transaction in transactions)
            {
                int year = DateTime.Now.Year;
                int day = transaction.StartDate.Day;
                DateTime seedDate = new DateTime(year, 1, day);
                for (int i = 0; i < totalCount; i++)
                {
                    try
                    {
                        DateTime dateOffset = frequency switch
                        {
                            Frequency.Daily => seedDate.AddDays(i),
                            Frequency.Weekly => seedDate.AddDays(7 * i),
                            Frequency.Monthly => seedDate.AddMonths(i),
                            Frequency.Yearly => seedDate.AddYears(i),
                            Frequency.Anticipated => transaction.StartDate,
                            _ => seedDate.AddMonths(i)
                        };

                        DateTime calculatedOffsetDate = offsetCalculationService.CalculateOffset(dateOffset).PlanDate;

                        planDates.Add(new PlanDate
                        {
                            Active = true,
                            Date = calculatedOffsetDate,
                            OriginalDate = transaction.StartDate,
                            Transaction = transaction
                        });
                    }
                    catch (Exception err)
                    {
                        logger.LogError(err, "Error generating plandate {TransactionName} {Iteration}", transaction.Name, i);
                    }
                }
            }

            return planDates;
        }
    }
}
