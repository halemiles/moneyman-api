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

namespace Moneyman.Tests
{
    [TestClass]
    public class DtpWeeklyGenerationTests
    {
        private Mock<ITransactionService> mockTransactionService;
        private Mock<ITransactionRepository> mockTransactionRepository;
        private Mock<IPlanDateRepository> mockPlanDateRepository;
        private Mock<IOffsetCalculationService> mockOffsetCalculationService;
        private Mock<IPaydayService> mockPaydayService;
        private Mock<ILogger<DtpService>> mockLogger;

        private readonly List<string> holidays = new List<string> 
        {
                "03-01-2022",
                "15-04-2022",
                "18-04-2022",
                "02-05-2022",
                "02-06-2022",
                "03-06-2022",
                "29-08-2022",
                "26-12-2022",
                "27-12-2022"
        };

        private DtpService NewDtpGenerationService() =>
            new DtpService(
                    mockTransactionRepository.Object,
                    mockPlanDateRepository.Object,
                    mockOffsetCalculationService.Object,
                    mockPaydayService.Object,
                    mockLogger.Object
            );

        [TestInitialize]
        public void SetUp()
        {
            mockTransactionService = new Mock<ITransactionService>();
            mockTransactionRepository = new Mock<ITransactionRepository>();
            mockPlanDateRepository = new Mock<IPlanDateRepository>();
            mockOffsetCalculationService = new Mock<IOffsetCalculationService>();
            mockPaydayService = new Mock<IPaydayService>();
            mockLogger = new Mock<ILogger<DtpService>>();

            mockOffsetCalculationService.Setup(x => x.CalculateOffset(It.IsAny<DateTime>()))
                .Returns(new CalculatedPlanDate());

            mockPaydayService.Setup(x => x.GetAll()).Returns(new List<Payday>());
        }

        [TestMethod]
        public void GenerateMonthly_WithInvalidTransactionId_ReturnsEmptyList()
        {
            // Arrange
            var sut = NewDtpGenerationService();
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
            var sut = NewDtpGenerationService();
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
            var sut = NewDtpGenerationService();
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
            var sut = NewDtpGenerationService();
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



        public void GenerateWeekly_WithAnticipatedTransactions_ShouldOnlyGenerateNonAnticipated_ReturnsSuccess()
        {
            // Arrange
            var sut = NewDtpGenerationService();
            var fixture = new Fixture();
            IEnumerable<Transaction> trans = new List<Transaction>
            {
                fixture.Build<Transaction>().With(f => f.IsAnticipated ,true).With(f => f.Name, "Trans 1").Create(),
                fixture.Build<Transaction>().With(f => f.IsAnticipated, false).With(f => f.Name, "Trans 2").Create()
            }.AsEnumerable();
            mockTransactionRepository.Setup(x => x.GetAll()).Returns(trans);

            // Act
            var result = sut.GenerateWeekly(1);

            // Assert
            result.Count.Should().Be(12);
            result.All(x => x.Transaction.Name == "Trans 1").Should().BeTrue();
            result.All(x => x.Transaction.IsAnticipated).Should().BeFalse();
            result.ShouldMatchSnapshot();
        }
    }
}
