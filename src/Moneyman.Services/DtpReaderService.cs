using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Moneyman.Domain;
using Moneyman.Interfaces;
using Moneyman.Models.Dtos;
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
        private readonly IMapper mapper;
        private readonly ILogger<DtpReaderService> logger;
        

        public DtpReaderService(
            ITransactionRepository transactionRepository,
            IPlanDateRepository planDateRepository,
            IOffsetCalculationService offsetCalculationService,
            IPaydayService paydayService,
            IDateTimeProvider dateTimeProvider,
            IMapper mapper,
            ILogger<DtpReaderService> logger
            
        ) 
        {
            this.transactionRepository = transactionRepository;
            this.planDateRepository = planDateRepository;
            this.offsetCalculationService = offsetCalculationService;
            this.paydayService = paydayService;
            this.datetimeProvider = dateTimeProvider;
            this.mapper = mapper;
            this.logger = logger;

        }

        public DtpDto GetCurrent()
        {
            var startDate = datetimeProvider.GetToday();
            var endDate = paydayService.GetNext().Date;

            logger.LogInformation(
                "Getting current DTP period {startDate} {endDate}",
                startDate,
                endDate
            );
            
            var planDates = planDateRepository 
                               .GetAll()
                               .Where(x => x.Date > startDate && x.Date < endDate)
                               .ToList();
            var mappedPlanDates = mapper.Map<List<PlanDateDto>>(planDates);
            return new DtpDto{
                PlanDates = mappedPlanDates,
                StartDate = startDate,
                EndDate = endDate
            };
        }

        public DtpDto GetOffset(int monthOffset = 0)
        {
            var startDate = paydayService.GetPrevious().Date.AddMonths(monthOffset);
            var endDate = paydayService.GetNext().Date.AddMonths(monthOffset);

            logger.LogInformation(
                "Getting current DTP period {startDate} {endDate}",
                startDate,
                endDate
            );
            
            var planDates = planDateRepository 
                               .GetAll()
                               .Where(x => x.Date > startDate && x.Date < endDate)
                               .ToList();
            var mappedPlanDates = mapper.Map<List<PlanDateDto>>(planDates);
            return new DtpDto{
                PlanDates = mappedPlanDates,
                StartDate = startDate,
                EndDate = endDate
            };
        }
    }
}