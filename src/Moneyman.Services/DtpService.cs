using System;
using System.Collections.Generic;
using System.Linq;
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

        public DtpService(
            ITransactionRepository transactionRepository,
            IPlanDateRepository planDateRepository,
            IOffsetCalculationService offsetCalculationService
        )
        {
            this.transactionRepository = transactionRepository;
            this.planDateRepository = planDateRepository;
            this.offsetCalculationService = offsetCalculationService;
        }

        public List<PlanDate> GenerateAll(int? transactionId)
        {
            
            transactionRepository.RemoveAll("PlanDates");
            List<PlanDate> planDates = GenerateMonthly(-1);  //TODO - PAss in a transaction ID if available
            planDates.AddRange(GenerateWeekly(-1));  //TODO - PAss in a transaction ID if available
            foreach(var planDate in planDates)
            {
                planDateRepository.Add(planDate);
                planDateRepository.Save(); //TODO - Try moving this out so we run batches
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

        public List<PlanDate> GenerateMonthly(int? transactionId)
        {
            var transactions = transactionRepository.GetAll().Where(x => x.Frequency == Frequency.Monthly);
            if(transactionId.HasValue)
            {
                transactions = transactions.Where(x => x.Id == transactionId);
            }
            List<PlanDate> planDates = new List<PlanDate>();
            foreach(var transaction in transactions)
            {
                for(int i=0;i<12;i++)
                {
                    DateTime startDate = new DateTime(transaction.StartDate.Year, 1, transaction.StartDate.Day); //Start at Jan
                    DateTime dateOffset = startDate.AddMonths(i);
                    
                    DateTime calculatedOffsetDate = offsetCalculationService.CalculateOffset(dateOffset).PlanDate; //TODO: Should this just return a date?
                    
                    var factory = new PlanDateFactory(transaction, calculatedOffsetDate);

                    planDates.Add(factory.Create());
                }
            }
            return planDates;
        }

        public List<PlanDate> GenerateWeekly(int? transactionId)
        {
            var transactions = transactionRepository.GetAll().Where(x => x.Frequency == Frequency.Weekly);
            if(transactionId.HasValue)
            {
                transactions = transactions.Where(x => x.Id == transactionId);
            }   
            
            List<PlanDate> planDates = new List<PlanDate>();
            foreach(var transaction in transactions)
            {
                for(int i=0;i<52;i++)
                {
                    DateTime startDate = new DateTime(transaction.StartDate.Year, 1, transaction.StartDate.Day); //Start at Jan
                    DateTime dateOffset = startDate.AddDays(7*i);
                    
                    DateTime calculatedOffsetDate = offsetCalculationService.CalculateOffset(dateOffset).PlanDate; //TODO: Should this just return a date?
                    
                    var factory = new PlanDateFactory(transaction, calculatedOffsetDate);

                    planDates.Add(factory.Create());
                }
            }
            return planDates;
        }

        public List<PlanDate> GenerateYearly(int? transactionId)
        {
            throw new System.NotImplementedException();
        }
    }
}