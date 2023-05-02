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

namespace YourProject.Tests
{
    [TestClass]
    public class DefaultPlanDateGenerationStrategyTests
    {
        private readonly Mock<ILogger<DtpService>> _mockLogger;
        private readonly Mock<ITransactionRepository> _mockTransactionRepository;
        private readonly Mock<IOffsetCalculationService> _mockOffsetCalculationService;
        private readonly DefaultPlanDateGenerationStrategy _generator;
        private readonly Mock<IPlanDateRepository> mockPlanDateRepository;

        public DefaultPlanDateGenerationStrategyTests()
        {
            _mockLogger = new Mock<ILogger<DtpService>>();
            _mockTransactionRepository = new Mock<ITransactionRepository>();
            _mockOffsetCalculationService = new Mock<IOffsetCalculationService>();
            mockPlanDateRepository = new Mock<IPlanDateRepository>();
            _generator = new DefaultPlanDateGenerationStrategy(
                _mockTransactionRepository.Object,
                mockPlanDateRepository.Object,
                _mockOffsetCalculationService.Object,
                _mockLogger.Object
            );
        }

        [TestMethod]
        public void Generate_WithValidInput_ReturnsPlanDates()
        {
            // Arrange
            int? transactionId = 1;
            Frequency frequency = Frequency.Monthly;

            var transactions = new List<Transaction>
            {
                new Transaction { Id = 1, Frequency = Frequency.Monthly, IsAnticipated = false, StartDate = new DateTime(2021, 1, 1), Name = "Test Transaction" }
            };

            _mockTransactionRepository.Setup(repo => repo.GetAll()).Returns(transactions.AsQueryable());

            _mockOffsetCalculationService.Setup(service => service.CalculateOffset(It.IsAny<DateTime>()))
                .Returns((DateTime inputDate) => new CalculatedPlanDate { PlanDate = inputDate });

            // Act
            var result = _generator.Generate(transactionId, frequency);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(12);
            result.Should().OnlyContain(planDate => planDate.Transaction == transactions[0]);
            result.Should().OnlyContain(planDate => planDate.Date.Month >= 1 && planDate.Date.Month <= 12);
        }
    }
}
