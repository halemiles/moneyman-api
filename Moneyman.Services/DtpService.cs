using System;
using System.Collections.Generic;
using Moneyman.Domain;
using Moneyman.Interfaces;
using Moneyman.Services.Interfaces;

namespace Moneyman.Services
{
    public class DtpService : IDtpService
    {

        private readonly ITransactionRepository transactionRepository;
        private readonly IPlanDateRepository planDateRepository;

        public DtpService(
            ITransactionRepository transactionRepository,
            IPlanDateRepository planDateRepository
        )
        {
            this.transactionRepository = transactionRepository;
            this.planDateRepository = planDateRepository;
        }

        public List<PlanDate> GenerateAll()
        {
            
            transactionRepository.RemoveAll("PlanDates");
            List<PlanDate> planDates = GenerateMonthly(-1);
            foreach(var planDate in planDates)
            {
                planDateRepository.Add(planDate);
                planDateRepository.Save(); //TODO - Try moving this out so we run batches
            }
            
            return planDates;
        }

        public List<PlanDate> GenerateDaily(int transactionId)
        {
            throw new System.NotImplementedException();
        }

        public List<PlanDate> GenerateForTransaction(int transactionId)
        {
            throw new System.NotImplementedException();
        }

        public List<PlanDate> GenerateMonthly(int transactionId)
        {
            var transactions = transactionRepository.GetAll();
            List<PlanDate> planDates = new List<PlanDate>();
            foreach(var transaction in transactions)
            {
                for(int i=0;i<12;i++)
                {
                    //TODO - Add to profile mapping
                    planDates.Add(new PlanDate()
                    {
                        Active = true,
                        Date = DateTime.Today, //TODO - Needs actual data
                        OriginalDate = DateTime.Today, //TODO - Needs actual data
                        YearGroup = 1, //TODO - Needs actual data
                        MonthGroup = 1, //TODO - Needs actual data
                        IsAnticipated = false, //TODO - Needs actual data
                        OrderId = 0, //TODO - Needs actual data
                        Transaction = new Transaction()
                        {
                            Name = transaction.Name,
                            Active = true,
                            
                        }
                    });
                }
            }
            return planDates;
        }

        public List<PlanDate> GenerateWeekly(int transactionId)
        {
            throw new System.NotImplementedException();
        }

        public List<PlanDate> GenerateYearly(int transactionId)
        {
            throw new System.NotImplementedException();
        }
    }
}