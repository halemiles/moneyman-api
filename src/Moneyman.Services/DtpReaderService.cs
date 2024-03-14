using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Moneyman.Domain;
using Moneyman.Domain.Models;
using Moneyman.Interfaces;
using Moneyman.Models.Dtos;
using Moneyman.Services.Factories;
using Moneyman.Services.Interfaces;

namespace Moneyman.Services
{
    public class DtpReaderService : IDtpReaderService
    {
        private readonly IPlanDateRepository planDateRepository;
        private readonly IPaydayService paydayService;
        private readonly IDateTimeProvider datetimeProvider;
        private readonly IMapper mapper;
        private readonly ILogger<DtpReaderService> logger;
        

        public DtpReaderService(
            IPlanDateRepository planDateRepository,
            IPaydayService paydayService,
            IDateTimeProvider dateTimeProvider,
            IMapper mapper,
            ILogger<DtpReaderService> logger
            
        ) 
        {
            this.planDateRepository = planDateRepository;
            this.paydayService = paydayService;
            this.datetimeProvider = dateTimeProvider;
            this.mapper = mapper;
            this.logger = logger;

        }

        public ApiResponse<DtpDto> GetCurrent()
        {
            var startDate = datetimeProvider.GetToday();
            DateTime endDate = DateTime.MinValue; 
            try
            {
                endDate = paydayService.GetNext().Date;
            }
            catch(Exception ex)
            {
                logger.LogError("Failed to get payday information");
                return ApiResponse.NotFound<DtpDto>("Could not find any paydays. Please ensure they have been generated");
            }

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
            return ApiResponse.Success<DtpDto>( new DtpDto{
                PlanDates = mappedPlanDates,
                StartDate = startDate,
                EndDate = endDate
            }, "Success");
        }

        public DtpDto GetOffset(int? monthOffset )
        {
            var offset = monthOffset ?? 0;
            var startDateRaw = paydayService.GetPrevious();
            var startDate = startDateRaw.Date.AddMonths(offset);
            var endDate = paydayService.GetNext().Date.AddMonths(offset);

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