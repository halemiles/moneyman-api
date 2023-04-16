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
    public class YearlyPlanDateGenerationStrategyTests
    {
        private readonly Mock<ILogger<DtpService>> _mockLogger;
        private readonly Mock<ITransactionRepository> _mockTransactionRepository;
        private readonly Mock<IOffsetCalculationService> _mockOffsetCalculationService;
        private readonly YearlyPlanDateGenerationStrategy _generator;
        private readonly Mock<IPlanDateRepository> mockPlanDateRepository;

        public YearlyPlanDateGenerationStrategyTests()
        {
            _mockLogger = new Mock<ILogger<DtpService>>();
            _mockTransactionRepository = new Mock<ITransactionRepository>();
            _mockOffsetCalculationService = new Mock<IOffsetCalculationService>();
            mockPlanDateRepository = new Mock<IPlanDateRepository>();
            _generator = new YearlyPlanDateGenerationStrategy(
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
            Frequency frequency = Frequency.Yearly;

            var transactions = new List<Transaction>
            {
                new Transaction { Id = 1, Frequency = Frequency.Yearly, IsAnticipated = false, StartDate = new DateTime(2021, 1, 1), Name = "Test Transaction" },
                new Transaction { Id = 2, Frequency = Frequency.Monthly, IsAnticipated = false, StartDate = new DateTime(2021, 1, 1), Name = "Test Transaction 2" }, //Shouldn't be generated
                new Transaction { Id = 3, Frequency = Frequency.Daily, IsAnticipated = true, StartDate = new DateTime(2021, 1, 1), Name = "Test Transaction 3" } //Shouldn't be generated
            };

            _mockTransactionRepository.Setup(repo => repo.GetAll()).Returns(transactions.AsQueryable());

            _mockOffsetCalculationService.Setup(service => service.CalculateOffset(It.IsAny<DateTime>()))
                .Returns((DateTime inputDate) => new CalculatedPlanDate { PlanDate = inputDate });

            // Act
            var result = _generator.Generate(transactionId, frequency);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result.Should().OnlyContain(planDate => planDate.Transaction == transactions[0]);
            result.Should().OnlyContain(planDate => planDate.Date.Month >= 1 && planDate.Date.Month <= 12);
        }
    }
}
