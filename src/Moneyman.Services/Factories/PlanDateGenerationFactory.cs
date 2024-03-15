using System;
using Microsoft.Extensions.Logging;
using Moneyman.Domain;
using Moneyman.Interfaces;

namespace Moneyman.Services.Factories
{
    public class PlanDateGenerationStrategyFactory
    {
        private readonly ITransactionRepository transactionRepository;
        private readonly IPlanDateRepository planDateRepository; //TODO: Is this required
        private readonly IOffsetCalculationService offsetCalculationService;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly ILogger<DtpService> logger;
        public PlanDateGenerationStrategyFactory(
            ITransactionRepository transactionRepository,
            IDateTimeProvider dateTimeProvider,
            IOffsetCalculationService offsetCalculationService,
            IPlanDateRepository planDateRepository,
            ILogger<DtpService> logger
        )
        {
            this.transactionRepository = transactionRepository;
            this.dateTimeProvider = dateTimeProvider;
            this.offsetCalculationService = offsetCalculationService;
            this.planDateRepository = planDateRepository;
            this.logger = logger;
        }
        public IPlanDateGenerationStrategy Create(Frequency frequency)
        {
            switch (frequency)
            {
                case Frequency.Weekly:
                case Frequency.Monthly:
                    return new DefaultPlanDateGenerationStrategy(
                        transactionRepository,
                        dateTimeProvider,
                        offsetCalculationService,
                        
                        logger
                    );
                case Frequency.Yearly:
                    return new YearlyPlanDateGenerationStrategy(
                        transactionRepository,
                        planDateRepository,
                        offsetCalculationService,
                        logger
                    );
                case Frequency.Daily:
                    return new DailyPlanDateGenerationStrategy(
                        transactionRepository,
                        planDateRepository,
                        offsetCalculationService,
                        logger
                    );
                default:
                    throw new ArgumentException($"Unsupported frequency: {frequency}");
            }
        }
    }
}