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
            result.Should().OnlyContain(planDate => planDate.Date != DateTime.MinValue);
        }

        [TestMethod]
        public void Generate_WhenOffsetShiftsDate_PlanDateReflectsShiftedDate()
        {
            // Arrange
            int? transactionId = 1;
            var originalDate = new DateTime(DateTime.Now.Year, 6, 1); // a Saturday in some years
            var shiftedDate = originalDate.AddDays(2);

            var transactions = new List<Transaction>
            {
                new Transaction { Id = 1, Frequency = Frequency.Monthly, StartDate = originalDate, Name = "Test Transaction" }
            };

            _mockTransactionRepository.Setup(repo => repo.GetAll()).Returns(transactions.AsQueryable());
            _mockOffsetCalculationService.Setup(service => service.CalculateOffset(It.IsAny<DateTime>()))
                .Returns((DateTime d) => new CalculatedPlanDate { PlanDate = shiftedDate });

            // Act
            var result = _generator.Generate(transactionId, Frequency.Monthly);

            // Assert
            result.Should().OnlyContain(planDate => planDate.Date == shiftedDate);
        }

        [TestMethod]
        public void Generate_WhenAnticipated_WithNullTransactionId_ReturnsAllAnticipatedTransactions()
        {
            // Arrange
            var transactions = new List<Transaction>
            {
                new Transaction { Id = 1, Frequency = Frequency.Anticipated, StartDate = new DateTime(2024, 3, 15), Name = "Anticipated 1" },
                new Transaction { Id = 2, Frequency = Frequency.Anticipated, StartDate = new DateTime(2024, 7, 20), Name = "Anticipated 2" }
            };

            _mockTransactionRepository.Setup(repo => repo.GetAll()).Returns(transactions.AsQueryable());
            _mockOffsetCalculationService.Setup(service => service.CalculateOffset(It.IsAny<DateTime>()))
                .Returns((DateTime d) => new CalculatedPlanDate { PlanDate = d });

            // Act
            var result = _generator.Generate(null, Frequency.Anticipated);

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(p => p.Transaction.Name == "Anticipated 1");
            result.Should().Contain(p => p.Transaction.Name == "Anticipated 2");
        }
    }
}
