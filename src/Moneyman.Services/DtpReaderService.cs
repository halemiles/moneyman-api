using System;
using System.Collections.Generic;
using System.Linq;
using Moneyman.Domain;
using Moneyman.Interfaces;
using Moneyman.Services.Factories;
using Moneyman.Services.Interfaces;

namespace Moneyman.Services
{
    public class DtpReaderService : IDtpReaderService
    {

        private readonly ITransactionRepository transactionRepository;
        private readonly IPlanDateRepository planDateRepository;
        private readonly IOffsetCalculationService offsetCalculationService;
        private readonly IPaydayService paydayService;
        private readonly IDateTimeProvider datetimeProvider;

        public DtpReaderService(
            ITransactionRepository transactionRepository,
            IPlanDateRepository planDateRepository,
            IOffsetCalculationService offsetCalculationService,
            IPaydayService paydayService,
            IDateTimeProvider dateTimeProvider
        )
        {
            this.transactionRepository = transactionRepository;
            this.planDateRepository = planDateRepository;
            this.offsetCalculationService = offsetCalculationService;
            this.paydayService = paydayService;
            this.datetimeProvider = dateTimeProvider;
        }

        public List<PlanDate> GetCurrent()
        {
            
            var startDate = datetimeProvider.GetToday();
            var endDate = paydayService.GetNext().Date;
            var planDates = planDateRepository 
                                .GetAll()
                                .Where(x => x.Date > startDate && x.Date < endDate)
                                .ToList();

            return planDates;
        }

    }
}