using System;
using Microsoft.Extensions.Logging;
using Moneyman.Domain;
using Moneyman.Interfaces;

namespace Moneyman.Services.Factories
{
    public class PlanDateGenerationStrategyFactory
    {
        private readonly ITransactionRepository transactionRepository;
        private readonly IPlanDateRepository planDateRepository;
        private readonly IOffsetCalculationService offsetCalculationService;
        private readonly ILogger<DtpService> logger;
        public PlanDateGenerationStrategyFactory(
            ITransactionRepository transactionRepository,
            IPlanDateRepository planDateRepository,
            IOffsetCalculationService offsetCalculationService,
            ILogger<DtpService> logger
        )
        {
            this.transactionRepository = transactionRepository;
            this.planDateRepository = planDateRepository;
            this.offsetCalculationService = offsetCalculationService;
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
                        planDateRepository,
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