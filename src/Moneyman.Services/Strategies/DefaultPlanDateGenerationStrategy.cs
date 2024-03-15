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
        private readonly IOffsetCalculationService offsetCalculationService;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly ILogger<DtpService> logger;
        public DefaultPlanDateGenerationStrategy(
            ITransactionRepository transactionRepository,
            IDateTimeProvider dateTimeProvider,
            IOffsetCalculationService offsetCalculationService,
            ILogger<DtpService> logger
        )
        {
            this.transactionRepository = transactionRepository;
            this.offsetCalculationService = offsetCalculationService;
            this.dateTimeProvider = dateTimeProvider;
            this.logger = logger;
        }

        public List<PlanDate> Generate(int? transactionId, Frequency frequency)
        {
            logger.LogInformation("Generating monthly");
            
            var transactions = transactionRepository.GetAll().Where(x => x.Frequency == Frequency.Monthly && !x.IsAnticipated);
            if(transactionId.HasValue)
            {
                transactions = transactions.Where(x => x.Id == transactionId);
            }

            List<PlanDate> planDates = new();
            int loopCount = frequency.ToFrequencyCount();

            foreach(var transaction in transactions)
            {
                for(int i=0;i<loopCount;i++)
                {
                    try
                    {
                        DateTime startDate = new DateTime(dateTimeProvider.GetNow().Year, 1, transaction.StartDate.Day); //Start at Jan
                        DateTime dateOffset = startDate.AddMonths(i);
                        
                        DateTime calculatedOffsetDate = offsetCalculationService.CalculateOffset(dateOffset).PlanDate; //TODO: Should this just return a date?
                        
                        var factory = new PlanDateFactory(transaction, calculatedOffsetDate);

                        planDates.Add(factory.Create());
                    }
                    catch(Exception err)
                    {
                        logger.LogError("Error generating monthly plandate {TransactionName} {month} {exceptionText}", transaction.Name, i, err.ToString());
                    }
                }
            }
            
            return planDates;
        }
    }
}