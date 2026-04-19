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
using Moneyman.Services.Factories;
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
                var response = await planDateRepository.Save(); //TODO - Try moving this out so we run batches
                Console.WriteLine($"Saved {response} plandates");
            }
            catch(Exception err)
            {
                logger.LogError("Failed saving plandates {ExceptionText}", err.ToString());

            }
            return ApiResponse.Success<List<PlanDate>>(planDates, "Successfully generated plandates");
        }

        public List<PlanDate> GenerateDaily(int? transactionId)
        {
            return GetGenerationStrategy().Generate(transactionId, Frequency.Daily);
        }

        private IPlanDateGenerationStrategy GetGenerationStrategy()
        {
            return new DefaultPlanDateGenerationStrategy(
                transactionRepository,
                planDateRepository,
                offsetCalculationService,
                logger
            );
        }

        public List<PlanDate> GenerateWeekly(int? transactionId)
        {
            logger.LogInformation("Generating weekly");
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
                    try
                    {
                        DateTime startDate = new DateTime(DateTime.Now.Year, 1, transaction.StartDate.Day); //Start at Jan of the current year
                        DateTime dateOffset = startDate.AddDays(7*i);

                        DateTime calculatedOffsetDate = offsetCalculationService.CalculateOffset(dateOffset).PlanDate; //TODO: Should this just return a date?

                        var factory = new PlanDateFactory(transaction, calculatedOffsetDate);

                        planDates.Add(factory.Create());
                    }
                    catch(Exception err)
                    {
                        logger.LogError("Error generating weekly plandate {TransactionName} {week} {exceptionText}", transaction.Name, i, err.ToString());
                    }
                }
            }
            return planDates;
        }

        public List<PlanDate> GenerateYearly(int? transactionId)
        {
            return GetGenerationStrategy().Generate(transactionId, Frequency.Yearly);
        }

        public List<PlanDate> GenerateAnticipated(int? transactionId)
        {
            return GetGenerationStrategy().Generate(transactionId, Frequency.Anticipated);
        }

        public List<PlanDate> GenerateMonthly(int? transactionId)
        {
            return GetGenerationStrategy().Generate(transactionId, Frequency.Monthly);
        }

        public ApiResponse<DtpDto> GetCurrent(int? startingValue)
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

            planDates = planDates.Where(x =>
                x.Transaction.Active
                && x.Date > startDate
                && x.Date < endDate
                && !x.Paid
            ).ToList();
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

        public ApiResponse<DtpDto>  GetOffset(int? monthOffset )
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
                               .Where(x => x.Transaction.Active && x.Date > startDate && x.Date < endDate && !x.Paid)
                               .ToList();
            var mappedPlanDates = planDateMapper.ToDtoList(planDates);
            return ApiResponse.Success<DtpDto>( new DtpDto{
                PlanDates = mappedPlanDates,
                StartDate = startDate,
                EndDate = endDate
            }, "Success");
        }
    }
}