using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moneyman.Interfaces;
using Moneyman.Domain;
using Moneyman.Domain.MapperProfiles;
using Moneyman.Domain.Models;
using Moneyman.Extensions;
using Moneyman.Models.DomainTransferObjects;
using Moneyman.Services.Interfaces;

namespace Moneyman.Services
{
    public class OffsetCalculationService : IOffsetCalculationService
    {
        private readonly IWeekdayService _weekdayService;
        private readonly IHolidayService _holidayService;
        private readonly IPaydayService _paydayService;
        private readonly IPlanDateRepository _planDateRepository;
        private readonly PlanDateMapper _planDateMapper;
        private readonly ILogger<OffsetCalculationService> _logger;

        public OffsetCalculationService(
            IWeekdayService weekdayService,
            IHolidayService holidayService,
            IPaydayService paydayService,
            IPlanDateRepository planDateRepository,
            PlanDateMapper planDateMapper,
            ILogger<OffsetCalculationService> logger
        )
        {
            _weekdayService = weekdayService;
            _holidayService = holidayService;
            _paydayService = paydayService;
            _planDateRepository = planDateRepository;
            _planDateMapper = planDateMapper;
            _logger = logger;
        }

        public OffsetCalculationService(
            IWeekdayService weekdayService,
            IHolidayService holidayService
        ) : this(weekdayService, holidayService, null, null, null, null)
        {
        }


        public CalculatedPlanDate CalculateOffset(DateTime dte)
        {
            var holidays = _holidayService.GenerateHolidays();
            var returnObject = new CalculatedPlanDate
            {
                OriginalPlanDate = dte
            };

            int offsetby = 0;
            bool found = false;
            int foundLoopCount = 0;

            //Iterate until we have found a suitable date
            while (!found)
            {
                //Check if this current iteration is on a weekend, monday or a bank holiday
                bool isWeekday = dte.IsWeekday();
                bool isBankHoliday = holidays.IsBankHoliday(dte);

                //If we have found a valid day (Tue-Fri and not on bank holiday)
                if(isWeekday && !isBankHoliday )
                {

                    returnObject.PlanDate = dte;
                    returnObject.IsBankHoliday = isBankHoliday;
                    returnObject.IsValid = true;
                    found = true;
                }
                else
                {
                    //Move forward by 1 day
                    //Note: Direct debits typically come out Mondays or Tuesdays
                    //      if they fall on a weekend or a bank holiday
                    dte = dte.AddDays(-1);
                    offsetby += -1;

                    returnObject.OffsetBy = offsetby;
                    returnObject.Reason = "On bank holiday or weekend";
                    returnObject.PlanDate = dte;

                }

                //This is to prevent infinate loops
                //TODO - Unit test to make sure this doesnt happen in the first place
                if(foundLoopCount >=10)
                {
                    found  = true;
                }

                foundLoopCount ++;

            }
            return returnObject;
        }

        public ApiResponse<DtpDto> GetPlanDatesByPeriod(int? monthOffset)
        {
            var offset = monthOffset ?? 0;
            var startDate = _paydayService.GetPrevious().Date.AddMonths(offset);
            var endDate = _paydayService.GetNext().Date.AddMonths(offset);

            _logger.LogInformation(
                "Getting DTP period {startDate} {endDate}",
                startDate,
                endDate
            );

            var planDates = _planDateRepository
                               .GetAll()
                               .Where(x => x.Transaction.Active && x.Date > startDate && x.Date < endDate)
                               .ToList();
            var mappedPlanDates = _planDateMapper.ToDtoList(planDates);

            return ApiResponse.Success(new DtpDto
            {
                PlanDates = mappedPlanDates,
                StartDate = startDate,
                EndDate = endDate
            }, "Success");
        }
    }
}