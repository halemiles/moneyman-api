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

namespace Moneyman.Tests
{
    [TestClass]
    public class DtpMonthlyGenerationTests
    {
        private Mock<ITransactionRepository> mockTransactionRepository;
        private Mock<IPlanDateRepository> mockPlanDateRepository;
        private Mock<IOffsetCalculationService> mockOffsetCalculationService;
        private Mock<ILogger<DtpService>> mockLogger;
        private DefaultPlanDateGenerationStrategy NewDtpGenerationService() =>
            new DefaultPlanDateGenerationStrategy(
                    mockTransactionRepository.Object,
                    mockPlanDateRepository.Object,
                    mockOffsetCalculationService.Object,
                    mockLogger.Object
            );

        [TestInitialize]
        public void SetUp()
        {
            mockTransactionRepository = new Mock<ITransactionRepository>();
            mockPlanDateRepository = new Mock<IPlanDateRepository>();
            mockOffsetCalculationService = new Mock<IOffsetCalculationService>();
            mockLogger = new Mock<ILogger<DtpService>>();

            mockOffsetCalculationService.Setup(x => x.CalculateOffset(It.IsAny<DateTime>()))
                .Returns(new CalculatedPlanDate());
        }

        [TestMethod]
        public void GenerateMonthly_WithInvalidTransactionId_ReturnsEmptyList()
        {
            // Arrange
            var sut = NewDtpGenerationService();
            IEnumerable<Transaction> trans = new List<Transaction>();
            mockTransactionRepository.Setup(x => x.GetAll()).Returns(trans);

            // Act
            var result = sut.Generate(null, Frequency.Monthly);

            // Assert
            result.Count.Should().Be(0);
        }

        [TestMethod]
        public void GenerateMonthly_WithValidMonthlyTransaction_ReturnsSuccess()
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
                    Frequency = Frequency.Monthly,
                    IsAnticipated = false
                }
            }.AsEnumerable();
            mockTransactionRepository.Setup(x => x.GetAll()).Returns(trans);

            // Act
            var result = sut.Generate(null, Frequency.Monthly);

            // Assert
            result.Count.Should().Be(12);
            result.All(x => x.Transaction.Name == "Trans 1").Should().BeTrue();
            result.ShouldMatchSnapshot();
        }

        [TestMethod]
        public void GenerateMonthly_WithMultipleTransactionFrequencies_ReturnsSuccess()
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
                    Frequency = Frequency.Monthly,
                    IsAnticipated = false
                },
                new Transaction
                {
                    Name = "Trans 2",
                    Amount = 100,
                    Active = true,
                    StartDate = new DateTime(2022,1,6),
                    Frequency = Frequency.Weekly,
                    IsAnticipated = false
                }
            }.AsEnumerable();
            mockTransactionRepository.Setup(x => x.GetAll()).Returns(trans);

            // Act
            var result = sut.Generate(null, Frequency.Monthly);

            // Assert
            result.Count.Should().Be(12);
            result.Any(x => x.Transaction.Name == "Trans 1").Should().BeTrue();
            result.Any(x => x.Transaction.Name == "Trans 2").Should().BeFalse();
            result.ShouldMatchSnapshot();
        }

        [TestMethod]
        public void GenerateMonthly_WithMultipleMonthlyTransactions_WhenGivenTransactionID_ReturnsSuccess()
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
                    Frequency = Frequency.Monthly,
                    IsAnticipated = false
                },
                new Transaction
                {
                    Id = 1,
                    Name = "Trans 2",
                    Amount = 100,
                    Active = true,
                    StartDate = new DateTime(2022,1,6),
                    Frequency = Frequency.Monthly,
                    IsAnticipated = false
                }
            }.AsEnumerable();
            mockTransactionRepository.Setup(x => x.GetAll()).Returns(trans);

            // Act
            var result = sut.Generate(1, Frequency.Monthly);

            // Assert
            result.Count.Should().Be(12);
            result.All(x => x.Transaction.Name == "Trans 2").Should().BeTrue();
            result.ShouldMatchSnapshot();
        }

        public void GenerateMonthly_WithAnticipatedTransactions_ShouldOnlyGenerateNonAnticipated_ReturnsSuccess()
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
            var result = sut.Generate(1, Frequency.Monthly);

            // Assert
            result.Count.Should().Be(12);
            result.All(x => x.Transaction.Name == "Trans 1").Should().BeTrue();
            result.All(x => x.Transaction.IsAnticipated).Should().BeFalse();
            result.ShouldMatchSnapshot();
        }

        public void GenerateMonthly_WhenCalculateOffsetThrows_ErrorIsLogged_ReturnsSuccess()
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
            mockOffsetCalculationService.SetupSequence(x => x.CalculateOffset(It.IsAny<DateTime>()))
                .Throws(new Exception())
                .Returns(new CalculatedPlanDate());
            // Act
            var result = sut.Generate(1, Frequency.Monthly);

            // Assert
            result.Should().NotBeNull();
            mockOffsetCalculationService.Verify(x => x.CalculateOffset(It.IsAny<DateTime>()), Times.Never);
            mockLogger.Verify(x => x.LogError(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()), Times.Once);
        }
    }
}
