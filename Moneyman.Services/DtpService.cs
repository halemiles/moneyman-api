using System.Collections.Generic;
using Moneyman.Domain;
using Moneyman.Interfaces;
using Moneyman.Services.Interfaces;

namespace Moneyman.Services
{
    public class DtpService : IDtpService
    {

        private ITransactionRepository transactionRepository;

        public DtpService(
            ITransactionRepository transactionRepository
        )
        {
            this.transactionRepository = transactionRepository;
        }

        public List<PlanDate> GenerateAll()
        {
            throw new System.NotImplementedException();
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
                        Transaction = new Transaction()
                        {
                            Name = transaction.Name
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