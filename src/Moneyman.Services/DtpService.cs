using System;
using System.Collections.Generic;
using System.Linq;
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

        public DtpService(
            ITransactionRepository transactionRepository,
            IPlanDateRepository planDateRepository,
            IOffsetCalculationService offsetCalculationService,
            IPaydayService paydayService,
            IDateTimeProvider dateTimeProvider,
            ILogger<DtpService> logger,
            PlanDateMapper planDateMapper
        )
        {
            this.transactionRepository = transactionRepository;
            this.planDateRepository = planDateRepository;
            this.offsetCalculationService = offsetCalculationService;
            this.paydayService = paydayService;
            this.dateTimeProvider = dateTimeProvider;
            this.logger = logger;
            this.planDateMapper = planDateMapper;
        }

        //TODO: Move this to a another class so we can unit test
        public ApiResponse<List<PlanDate>> GenerateAll(int? transactionId)
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
            planDates.AddRange(GenerateMonthly(transactionId));
            planDates.AddRange(GenerateWeekly(transactionId));
            planDates.AddRange(GenerateYearly(transactionId));
            planDates.AddRange(GenerateAnticipated(transactionId));
            planDates.AddRange(GenerateDaily(transactionId));
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
            return ApiResponse.Success<List<PlanDate>>(planDates, "Successfully generated plandates");
        }

        public List<PlanDate> GenerateDaily(int? transactionId) =>
            GetGenerationStrategy().Generate(transactionId, Frequency.Daily);

        public List<PlanDate> GenerateWeekly(int? transactionId) =>
            GetGenerationStrategy().Generate(transactionId, Frequency.Weekly);

        public List<PlanDate> GenerateYearly(int? transactionId) =>
            GetGenerationStrategy().Generate(transactionId, Frequency.Yearly);

        public List<PlanDate> GenerateAnticipated(int? transactionId) =>
            GetGenerationStrategy().Generate(transactionId, Frequency.Anticipated);

        public List<PlanDate> GenerateMonthly(int? transactionId) =>
            GetGenerationStrategy().Generate(transactionId, Frequency.Monthly);

        public IPlanDateGenerationStrategy GetGenerationStrategy() =>
            new DefaultPlanDateGenerationStrategy(
                transactionRepository,
                planDateRepository,
                offsetCalculationService,
                logger
            );

        public ApiResponse<DtpDto> GetCurrent(int? startingValue, int? bankAccountId)
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

            logger.LogInformation(
                "Getting current DTP period {startDate} {endDate}",
                startDate,
                endDate
            );

            var planDates = planDateRepository
                               .GetAll();

            if (!planDates.Any())
            {
                logger.LogWarning("Plandate list is empty");
                return ApiResponse.NoContent<DtpDto>("Plandate list is empty. Please regenerate plandates");
            }
            
            var filteredPlanDates = planDates.Where(x =>
                x.Transaction.Active
                && x.Date > startDate
                && x.Date < endDate
                && (!bankAccountId.HasValue
                    || bankAccountId == 0
                    || x.Transaction.BankAccountId == bankAccountId
                )
            ).ToList();

            var mappedPlanDates = planDateMapper.ToDtoList(filteredPlanDates);
            var amountDue = mappedPlanDates.Sum(x => x.Amount);
            var weeksDivisor = Math.Max((endDate - startDate).Days / 7, 1);
            var startValue = startingValue ?? 0;
            var remaining = startValue - amountDue;

            return ApiResponse.Success(new DtpDto
            {
                PlanDates = mappedPlanDates,
                StartDate = startDate,
                EndDate = endDate,
                WeeksRemaining = weeksDivisor,
                AmountDue = amountDue,
                SpendPerWeek = remaining / weeksDivisor,
                Remaining = remaining
            }, "Success");
        }

        public ApiResponse<DtpDto> GetOffset(int? monthOffset) =>
            offsetCalculationService.GetPlanDatesByPeriod(monthOffset);
    }
}