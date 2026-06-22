using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moneyman.Domain;
using Moneyman.Domain.MapperProfiles;
using Moneyman.Domain.Models;
using Moneyman.Interfaces;
using Moneyman.Models.DomainTransferObjects;
using Moneyman.Services.Interfaces;

namespace Moneyman.Services
{
    public class DtpService : IDtpService
    {
        private readonly ITransactionRepository transactionRepository;
        private readonly IPlanDateRepository planDateRepository;
        private readonly IOffsetCalculationService offsetCalculationService;
        private readonly IPaydayService paydayService;
        private readonly ILogger<DtpService> logger;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly PlanDateMapper planDateMapper;
        private readonly IPlanDateGenerationStrategy generationStrategy;

        public DtpService(
            ITransactionRepository transactionRepository,
            IPlanDateRepository planDateRepository,
            IOffsetCalculationService offsetCalculationService,
            IPaydayService paydayService,
            IDateTimeProvider dateTimeProvider,
            ILogger<DtpService> logger,
            PlanDateMapper planDateMapper,
            IPlanDateGenerationStrategy generationStrategy
        )
        {
            this.transactionRepository = transactionRepository;
            this.planDateRepository = planDateRepository;
            this.offsetCalculationService = offsetCalculationService;
            this.paydayService = paydayService;
            this.dateTimeProvider = dateTimeProvider;
            this.logger = logger;
            this.planDateMapper = planDateMapper;
            this.generationStrategy = generationStrategy;
        }

        public async Task<ApiResponse<List<PlanDate>>> GenerateAll(int? transactionId)
        {
            if(!paydayService.GetAll().Any())
            {
                return ApiResponse.NotFound<List<PlanDate>>("No paydays found. Please regenerate");
            }

            var currentYear = dateTimeProvider.GetToday().Year;
            if(!transactionRepository.GetAll().Any(x => x.StartDate.Year == currentYear))
            {
                logger.LogWarning("No transactions with start date in current year {CurrentYear}", currentYear);
                return ApiResponse.NotFound<List<PlanDate>>($"No transactions exist which start in the current year ({currentYear}). Please add or update transactions.");
            }

            logger.LogInformation("Removing existing plan dates");
            transactionRepository.RemoveAll("PlanDates");

            List<PlanDate> planDates = new();
            foreach (var frequency in new[] { Frequency.Monthly, Frequency.Weekly, Frequency.Yearly, Frequency.Anticipated, Frequency.Daily })
            {
                planDates.AddRange(generationStrategy.Generate(transactionId, frequency));
            }

            foreach(var planDate in planDates)
            {
                planDateRepository.Add(planDate);
            }

            try
            {
                var response = await planDateRepository.Save();
                logger.LogInformation("Saved {PlanDateCount} plandates", response);
            }
            catch(Exception err)
            {
                logger.LogError("Failed saving plandates {ExceptionText}", err.ToString());
            }
            return ApiResponse.Success<List<PlanDate>>(planDates, "Successfully generated plandates");
        }

        public ApiResponse<DtpDto> GetCurrent(int? startingValue, int? bankAccountId = null)
        {
            var startDate = dateTimeProvider.GetToday();
            DateTime endDate = DateTime.MinValue;
            try
            {
                endDate = paydayService.GetNext().Date;
            }
            catch(Exception)
            {
                logger.LogError("Failed to get payday information");
                return ApiResponse.NotFound<DtpDto>("Could not find any paydays. Please ensure they have been generated");
            }

            var planDates = planDateRepository.GetAll();

            if (!planDates.Any())
            {
                logger.LogWarning("Plandate list is empty");
                return ApiResponse.NoContent<DtpDto>("Plandate list is empty. Please regenerate plandates");
            }

            planDates = planDates.Where(x => IsDueBetween(x, startDate, endDate, bankAccountId)).ToList();
            var mappedPlanDates = planDateMapper.ToDtoList(planDates);
            var amountDue = mappedPlanDates.Sum(x => x.Amount);
            var weeksRemaining = WeeksRemaining(startDate, endDate);
            var weekDivisder = weeksRemaining == 0 ? 1 : weeksRemaining;
            var startValue = startingValue ?? 0;
            return ApiResponse.Success<DtpDto>( new DtpDto{
                PlanDates = mappedPlanDates,
                StartDate = startDate,
                EndDate = endDate,
                WeeksRemaining = weekDivisder,
                AmountDue = amountDue,
                SpendPerWeek = ((startValue - amountDue) / weekDivisder ),
                Remaining = startValue - amountDue
            }, "Success");
        }

        private int WeeksRemaining(DateTime start, DateTime end){
            return (end - start).Days / 7;
        }

        private static bool IsDueBetween(PlanDate planDate, DateTime startDate, DateTime endDate, int? bankAccountId = null)
        {
            return planDate.Transaction != null
                && planDate.Transaction.Active
                && planDate.Date > startDate
                && planDate.Date < endDate
                && !planDate.Paid
                && (bankAccountId == null || planDate.Transaction.BankAccountId == bankAccountId.Value);
        }

        // Like IsDueBetween but without the date window: used by the "full" view,
        // which returns every active, unpaid plan date across the whole span.
        private static bool IsDue(PlanDate planDate, int? bankAccountId = null)
        {
            return planDate.Transaction != null
                && planDate.Transaction.Active
                && !planDate.Paid
                && (bankAccountId == null || planDate.Transaction.BankAccountId == bankAccountId.Value);
        }

        public ApiResponse<DtpDto> GetOffset(int? monthOffset, int? bankAccountId = null)
        {
            var offset = monthOffset ?? 0;
            var startDateRaw = paydayService.GetPrevious();
            var endDateRaw = paydayService.GetNext();

            if (startDateRaw == null || endDateRaw == null)
            {
                logger.LogError("Failed to get payday information");
                return ApiResponse.NotFound<DtpDto>("Could not find any paydays. Please ensure they have been generated");
            }

            var startDate = startDateRaw.Date.AddMonths(offset);
            var endDate = endDateRaw.Date.AddMonths(offset);

            var planDates = planDateRepository
                               .GetAll()
                               .Where(x => IsDueBetween(x, startDate, endDate, bankAccountId))
                               .ToList();
            var mappedPlanDates = planDateMapper.ToDtoList(planDates);
            return ApiResponse.Success<DtpDto>( new DtpDto{
                PlanDates = mappedPlanDates,
                StartDate = startDate,
                EndDate = endDate
            }, "Success");
        }

        public ApiResponse<DtpDto> GetAll(int? bankAccountId = null)
        {
            // The whole generated span: every active, unpaid plan date regardless of
            // period, so one-off Anticipated bills dated outside the current payday
            // period are still visible.
            var planDates = planDateRepository
                               .GetAll()
                               .Where(x => IsDue(x, bankAccountId))
                               .ToList();
            var mappedPlanDates = planDateMapper.ToDtoList(planDates);
            return ApiResponse.Success<DtpDto>( new DtpDto{
                PlanDates = mappedPlanDates,
                StartDate = planDates.Count > 0 ? planDates.Min(x => x.Date) : DateTime.MinValue,
                EndDate = planDates.Count > 0 ? planDates.Max(x => x.Date) : DateTime.MinValue
            }, "Success");
        }
    }
}
