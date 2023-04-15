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
    public class DailyPlanDateGenerationStrategy : IPlanDateGenerationStrategy
    {
        private readonly ITransactionRepository transactionRepository;
        private readonly IPlanDateRepository planDateRepository;
        private readonly IOffsetCalculationService offsetCalculationService;
        private readonly ILogger<DtpService> logger;
        public DailyPlanDateGenerationStrategy(
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
            logger.LogInformation("Generating monthly");
            
            var transactions = transactionRepository.GetAll().Where(x => x.Frequency == Frequency.Daily && !x.IsAnticipated);
            if(transactionId.HasValue)
            {
                transactions = transactions.Where(x => x.Id == transactionId);
            }

            List<PlanDate> planDates = new();
            int loopCount = frequency.ToFrequencyCount();

            foreach(var transaction in transactions)
            {
                for(int i=0;i<365;i++)
                {
                    try
                    {
                        DateTime startDate = new DateTime(transaction.StartDate.Year, 1, transaction.StartDate.Day); //Start at Jan
                        DateTime dateOffset = startDate.AddDays(i);
                        
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