using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moneyman.Domain;
using Moneyman.Interfaces;
using Moneyman.Services.Extentions;
using Moneyman.Services.Factories;

namespace Moneyman.Services
{
    public class DefaultPlanDateGenerationStrategy : IPlanDateGenerationStrategy
    {
        private readonly ITransactionRepository transactionRepository;
        private readonly IPlanDateRepository planDateRepository;
        private readonly IOffsetCalculationService offsetCalculationService;
        private readonly ILogger<DtpService> logger;
        private const int TotalPlanDateYears = 2;
        public DefaultPlanDateGenerationStrategy(
            ITransactionRepository transactionRepository,
            IPlanDateRepository planDateRepository,
            IOffsetCalculationService offsetCalculationService,
            ILogger<DtpService> logger
        )
        {
            this.transactionRepository = transactionRepository;
            this.planDateRepository = planDateRepository;
            this.offsetCalculationService = offsetCalculationService;
            this.logger = logger;
        }

        public List<PlanDate> Generate(int? transactionId, Frequency frequency)
        {
            logger.LogInformation("Generating {Frequency}", frequency);

            var transactions = transactionRepository.GetAll().Where(x => x.Frequency == frequency && !x.IsAnticipated);
            if(transactionId.HasValue)
            {
                transactions = transactions.Where(x => x.Id == transactionId);
            }

            List<PlanDate> planDates = new();
            int totalCount = frequency.ToFrequencyCount() * TotalPlanDateYears;

            foreach(var transaction in transactions)
            {
                DateTime startDate = new DateTime(DateTime.Now.Year, 1, transaction.StartDate.Day); //Start at Jan
                for(int i=0;i<totalCount;i++)
                {
                    try
                    {
                        DateTime dateOffset = AddFrequencyOffset(startDate, frequency, i);

                        DateTime calculatedOffsetDate = offsetCalculationService.CalculateOffset(dateOffset).PlanDate;

                        var factory = new PlanDateFactory(transaction, calculatedOffsetDate);

                        planDates.Add(factory.Create());
                    }
                    catch(Exception err)
                    {
                        logger.LogError("Error generating {Frequency} plandate {TransactionName} {period} {exceptionText}", frequency, transaction.Name, i, err.ToString());
                    }
                }
            }

            return planDates;
        }

        private static DateTime AddFrequencyOffset(DateTime startDate, Frequency frequency, int offset) => frequency switch
        {
            Frequency.Daily => startDate.AddDays(offset),
            Frequency.Weekly => startDate.AddDays(7 * offset),
            _ => startDate.AddMonths(offset)
        };
    }
}