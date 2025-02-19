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
            return new DefaultPlanDateGenerationStrategy(
                transactionRepository,
                planDateRepository,
                offsetCalculationService,
                logger
            );
        }
    }
}