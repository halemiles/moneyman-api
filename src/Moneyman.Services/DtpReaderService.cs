using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
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

        public DtpReaderService(
            ITransactionRepository transactionRepository,
            IPlanDateRepository planDateRepository,
            IOffsetCalculationService offsetCalculationService,
            IPaydayService paydayService,
            IDateTimeProvider dateTimeProvider,
            IMapper mapper
        )
        {
            this.transactionRepository = transactionRepository;
            this.planDateRepository = planDateRepository;
            this.offsetCalculationService = offsetCalculationService;
            this.paydayService = paydayService;
            this.datetimeProvider = dateTimeProvider;
            this.mapper = mapper;
        }

        public List<PlanDateDto> GetCurrent()
        {
            
            var startDate = datetimeProvider.GetToday();
            var endDate = paydayService.GetNext().Date;
            var planDates = planDateRepository 
                               .GetAll()
                               .Where(x => x.Date > startDate && x.Date < endDate)
                               .ToList();
            var mappedPlanDates = mapper.Map<List<PlanDateDto>>(planDates);
            return mappedPlanDates;
            //return new List<PlanDateDto>();
        }

    }
}