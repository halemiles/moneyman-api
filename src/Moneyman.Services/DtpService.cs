using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moneyman.Domain;
using Moneyman.Interfaces;
using Moneyman.Services.Factories;
using Moneyman.Services.Interfaces;

namespace Moneyman.Services
{
    public class DtpService : IDtpService
    {

        private readonly ITransactionRepository transactionRepository;
        private readonly IPlanDateRepository planDateRepository;
        private readonly IOffsetCalculationService offsetCalculationService;
        private readonly ILogger<DtpService> logger;

        public DtpService(
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

        //TODO: Move this to a another class so we can unit test
        public List<PlanDate> GenerateAll(int? transactionId)
        {
            logger.LogInformation("Removing existing plan dates");
            transactionRepository.RemoveAll("PlanDates");
            
            List<PlanDate> planDates = new();
            planDates.AddRange(GetGenerationStrategy("monthly").Generate(null));  //TODO - PAss in a transaction ID if available
            planDates.AddRange(GenerateWeekly(null));  //TODO - PAss in a transaction ID if available

            foreach(var planDate in planDates)
            {
                planDateRepository.Add(planDate);                
            }
            
            try
            {
                planDateRepository.Save(); //TODO - Try moving this out so we run batches
            }
            catch(Exception err)
            {
                logger.LogError("Failed saving plandates {ExceptionText}", err.ToString());
                
            }
            return planDates;
        }

        public List<PlanDate> GenerateDaily(int? transactionId)
        {
            throw new System.NotImplementedException();
        }

        public List<PlanDate> GenerateForTransaction(int? transactionId)
        {
            throw new System.NotImplementedException();
        }

        public IPlanDateGenerationStrategy GetGenerationStrategy(string strategyName)
        {
            IPlanDateGenerationStrategy generationStrategy;
            switch(strategyName)
            {
                case "monthly":
                    generationStrategy = new GenerateMonthlyPlanDateStrategy(transactionRepository, planDateRepository, offsetCalculationService, logger);
                    break;
                default:
                    // TODO: Maybe this needs to be a yearly for safety? Generate a single plan date
                    generationStrategy = new GenerateMonthlyPlanDateStrategy(transactionRepository, planDateRepository, offsetCalculationService, logger);
                    break;
            }
            
            return generationStrategy;
        }

        public List<PlanDate> GenerateWeekly(int? transactionId)
        {
            logger.LogInformation("Generating weekly");
            var transactions = transactionRepository.GetAll().Where(x => x.Frequency == Frequency.Weekly && !x.IsAnticipated);
            if(transactionId.HasValue)
            {
                transactions = transactions.Where(x => x.Id == transactionId);
            }   
            
            List<PlanDate> planDates = new List<PlanDate>();
            foreach(var transaction in transactions)
            {
                for(int i=0;i<52;i++)
                {
                    try
                    {
                        DateTime startDate = new DateTime(transaction.StartDate.Year, 1, transaction.StartDate.Day); //Start at Jan
                        DateTime dateOffset = startDate.AddDays(7*i);
                        
                        DateTime calculatedOffsetDate = offsetCalculationService.CalculateOffset(dateOffset).PlanDate; //TODO: Should this just return a date?
                        
                        var factory = new PlanDateFactory(transaction, calculatedOffsetDate);

                        planDates.Add(factory.Create());
                    }
                    catch(Exception err)
                    {
                        logger.LogError("Error generating weekly plandate {TransactionName} {week} {exceptionText}", transaction.Name, i, err.ToString());
                    }
                }
            }
            return planDates;
        }

        public List<PlanDate> GenerateYearly(int? transactionId)
        {
            throw new System.NotImplementedException();
        }

        public List<PlanDate> GenerateMonthly(int? transactionId)
        {
            return GetGenerationStrategy("monthly").Generate(transactionId);
        }
    }
}