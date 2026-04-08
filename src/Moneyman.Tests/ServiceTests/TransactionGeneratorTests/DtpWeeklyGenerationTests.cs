using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moneyman.Interfaces;
using Moneyman.Services;
using Moneyman.Domain;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using System;
using Snapper;
using Microsoft.Extensions.Logging;
using AutoFixture;
using Moneyman.Services.Interfaces;
using Moneyman.Domain.MapperProfiles;

namespace Moneyman.Tests
{
    [TestClass]
    public class DtpWeeklyGenerationTests
    {
        private Mock<ITransactionRepository> mockTransactionRepository;
        private Mock<IPlanDateRepository> mockPlanDateRepository;
        private Mock<IOffsetCalculationService> mockOffsetCalculationService;
        private Mock<IPaydayService> mockPaydayService;
        private Mock<IDateTimeProvider> mockDateTimeProvider;
        private PlanDateMapper mockPlanDateMapper;
        private Mock<ILogger<DtpService>> mockLogger;

        private DtpService NewDtpService() =>
            new DtpService(
                    mockTransactionRepository.Object,
                    mockPlanDateRepository.Object,
                    mockOffsetCalculationService.Object,
                    mockPaydayService.Object,
                    mockDateTimeProvider.Object,
                    mockLogger.Object,
                    mockPlanDateMapper
            );

        [TestInitialize]
        public void SetUp()
        {
            mockPlanDateRepository = new Mock<IPlanDateRepository>();
            mockTransactionRepository = new Mock<ITransactionRepository>();
            mockOffsetCalculationService = new Mock<IOffsetCalculationService>();
            mockPaydayService = new Mock<IPaydayService>();
            mockDateTimeProvider = new Mock<IDateTimeProvider>();
            mockLogger = new Mock<ILogger<DtpService>>();
            mockPlanDateMapper = new PlanDateMapper();

            mockOffsetCalculationService.Setup(x => x.CalculateOffset(It.IsAny<DateTime>()))
                .Returns((DateTime d) => new CalculatedPlanDate { PlanDate = d });

            mockPaydayService.Setup(x => x.GetAll()).Returns(new List<Payday>());
        }

        [TestMethod]
        public void GenerateMonthly_WithInvalidTransactionId_ReturnsEmptyList()
        {
            // Arrange
            var sut = NewDtpService();
            IEnumerable<Transaction> trans = new List<Transaction>();
            mockTransactionRepository.Setup(x => x.GetAll()).Returns(trans);

            // Act
            var result = sut.GenerateWeekly(null);

            // Assert
            result.Count.Should().Be(0);
        }

        [TestMethod]
        public void GenerateWeekly_WithValidWeeklyTransaction_ReturnsSuccess()
        {
            // Arrange
            var sut = NewDtpService();
            IEnumerable<Transaction> trans = new List<Transaction>
            {
                new Transaction
                {
                    Name = "Trans 1",
                    Amount = 100,
                    Active = true,
                    StartDate = new DateTime(2022,1,1),
                    Frequency = Frequency.Weekly
                }
            }.AsEnumerable();
            mockTransactionRepository.Setup(x => x.GetAll()).Returns(trans);

            // Act
            var result = sut.GenerateWeekly(null);

            // Assert
            result.Count.Should().Be(52);
            result.All(x => x.Transaction.Name == "Trans 1").Should().BeTrue();
            result.ShouldMatchSnapshot();
        }

        [TestMethod]
        public void GenerateWeekly_WithMultipleTransactionFrequencies_ReturnsSuccess()
        {
            // Arrange
            var sut = NewDtpService();
            IEnumerable<Transaction> trans = new List<Transaction>
            {
                new Transaction
                {
                    Name = "Trans 1",
                    Amount = 100,
                    Active = true,
                    StartDate = new DateTime(2022,1,1),
                    Frequency = Frequency.Monthly
                },
                new Transaction
                {
                    Name = "Trans 2",
                    Amount = 100,
                    Active = true,
                    StartDate = new DateTime(2022,1,6),
                    Frequency = Frequency.Weekly
                }
            }.AsEnumerable();
            mockTransactionRepository.Setup(x => x.GetAll()).Returns(trans);

            // Act
            var result = sut.GenerateWeekly(null);

            // Assert
            result.Count.Should().Be(52);
            result.Any(x => x.Transaction.Name == "Trans 1").Should().BeFalse();
            result.Any(x => x.Transaction.Name == "Trans 2").Should().BeTrue();
            result.ShouldMatchSnapshot();
        }

        [TestMethod]
        public void GenerateWeekly_WithMultipleWeeklyTransactions_WhenTransactionIdSupplied_ReturnsSuccess()
        {
            // Arrange
            var sut = NewDtpService();
            IEnumerable<Transaction> trans = new List<Transaction>
            {
                new Transaction
                {
                    Id = 0,
                    Name = "Trans 1",
                    Amount = 100,
                    Active = true,
                    StartDate = new DateTime(2022,1,1),
                    Frequency = Frequency.Weekly
                },
                new Transaction
                {
                    Id = 1,
                    Name = "Trans 2",
                    Amount = 100,
                    Active = true,
                    StartDate = new DateTime(2022,1,6),
                    Frequency = Frequency.Weekly
                }
            }.AsEnumerable();
            mockTransactionRepository.Setup(x => x.GetAll()).Returns(trans);

            // Act
            var result = sut.GenerateWeekly(1);

            // Assert
            result.Count.Should().Be(52);
            result.All(x => x.Transaction.Name == "Trans 2").Should().BeTrue();
            result.ShouldMatchSnapshot();
        }



        [TestMethod]
        public void GenerateWeekly_WithAnticipatedFrequencyTransactions_ShouldNotAppearInWeeklyGeneration_ReturnsSuccess()
        {
            // Arrange
            var sut = NewDtpService();
            var fixture = new Fixture();
            IEnumerable<Transaction> trans = new List<Transaction>
            {
                fixture.Build<Transaction>().With(f => f.Frequency, Frequency.Anticipated).With(f => f.Name, "Trans 1").Create(),
                fixture.Build<Transaction>().With(f => f.Frequency, Frequency.Weekly).With(f => f.Name, "Trans 2").Create()
            }.AsEnumerable();
            mockTransactionRepository.Setup(x => x.GetAll()).Returns(trans);

            // Act
            var result = sut.GenerateWeekly(null);

            // Assert
            result.Any(x => x.Transaction.Name == "Trans 1").Should().BeFalse();
            result.Any(x => x.Transaction.Name == "Trans 2").Should().BeTrue();
        }
    }
}
