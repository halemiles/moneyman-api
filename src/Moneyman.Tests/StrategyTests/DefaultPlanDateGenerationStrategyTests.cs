using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moneyman.Interfaces;
using Moneyman.Services;
using Moneyman.Domain;
using System;

namespace Moneyman.Tests.StrategyTests
{
    [TestClass]
    public class DefaultPlanDateGenerationStrategyTests
    {
        private readonly Mock<ITransactionRepository> _mockTransactionRepository;
        private readonly Mock<IOffsetCalculationService> _mockOffsetCalculationService;
        private readonly DefaultPlanDateGenerationStrategy _generator;

        public DefaultPlanDateGenerationStrategyTests()
        {
            var mockLogger = new Mock<ILogger<DtpService>>();
            var mockPlanDateRepository = new Mock<IPlanDateRepository>();
            _mockTransactionRepository = new Mock<ITransactionRepository>();
            _mockOffsetCalculationService = new Mock<IOffsetCalculationService>();

            _generator = new DefaultPlanDateGenerationStrategy(
                _mockTransactionRepository.Object,
                mockPlanDateRepository.Object,
                _mockOffsetCalculationService.Object,
                mockLogger.Object
            );
        }

        [TestMethod]
        [DataRow(Frequency.Monthly, 24)]
        [DataRow(Frequency.Yearly, 2)]
        [DataRow(Frequency.Weekly, 104)]
        [DataRow(Frequency.Daily, 730)]
        [DataRow(Frequency.Anticipated, 1)]
        public void Generate_WhenMonthly_WithValidInput_ReturnsPlanDates(Frequency frequency, int expectedRecordCount)
        {
            // Arrange
            int? transactionId = 1;

            var transactions = new List<Transaction>
            {
                new Transaction { Id = 1, Frequency = frequency, StartDate = new DateTime(2021, 1, 1), Name = "Test Transaction" }
            };

            _mockTransactionRepository.Setup(repo => repo.GetAll()).Returns(transactions.AsQueryable());

            _mockOffsetCalculationService.Setup(service => service.CalculateOffset(It.IsAny<DateTime>()))
                .Returns((DateTime inputDate) => new CalculatedPlanDate { PlanDate = inputDate });

            // Act
            var result = _generator.Generate(transactionId, frequency);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(expectedRecordCount);
            result.Should().OnlyContain(planDate => planDate.Transaction == transactions[0]);
            result.Should().OnlyContain(planDate => planDate.Date.Month >= 1 && planDate.Date.Month <= 12);
        }
    }
}
