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
using AutoFixture;
using Microsoft.Extensions.Logging;
using Moneyman.Services.Interfaces;
using Moneyman.Domain.MapperProfiles;
using System.Threading.Tasks;

namespace Moneyman.Tests
{
    [TestClass]
    public class GenerateAllTests
    {


        private Mock<ITransactionRepository> mockTransactionRepository;
        private Mock<IPlanDateRepository> mockPlanDateRepository;
        private Mock<IOffsetCalculationService> mockOffsetCalculationService;
        private Mock<IPaydayService> mockPaydayService;
        private Mock<IDateTimeProvider> mockDateTimeProvider;
        private PlanDateMapper planDateMapper;
        private Mock<ILogger<DtpService>> mockLogger;
        private Mock<IPlanDateGenerationStrategy> mockGenerationStrategy;

        private DtpService NewDtpService() =>
            new DtpService(
                    mockTransactionRepository.Object,
                    mockPlanDateRepository.Object,
                    mockOffsetCalculationService.Object,
                    mockPaydayService.Object,
                    mockDateTimeProvider.Object,
                    mockLogger.Object,
                    planDateMapper,
                    mockGenerationStrategy.Object
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
            mockGenerationStrategy = new Mock<IPlanDateGenerationStrategy>();
            mockGenerationStrategy.Setup(x => x.Generate(It.IsAny<int?>(), It.IsAny<Frequency>())).Returns(new List<PlanDate>());
            planDateMapper = new PlanDateMapper();

            mockOffsetCalculationService.Setup(x => x.CalculateOffset(It.IsAny<DateTime>()))
                .Returns(new CalculatedPlanDate());

            mockPaydayService.Setup(x => x.GetAll()).Returns(new List<Payday>(){new Payday()});
        }

        [TestMethod]
        public async System.Threading.Tasks.Task GenerateAll_WhenNoPaydaysExist_ReturnsNotFound()
        {
            // Arrange
            var sut = NewDtpService();
            mockPaydayService.Setup(x => x.GetAll()).Returns(new List<Payday>());
            // Act
            var result = await sut.GenerateAll(null);

            // Assert
            result.StatusCode.Should().Be(StatusCode.NotFound);
        }

        [TestMethod]
        public async System.Threading.Tasks.Task GenerateAll_WhenNoTransactionsStartInCurrentYear_ReturnsNotFound()
        {
            // Arrange
            var sut = NewDtpService();
            const int testYear = 2024;
            mockDateTimeProvider.Setup(x => x.GetToday()).Returns(new DateTime(testYear, 6, 15));

            // Create transactions with start dates NOT in the current year
            IEnumerable<Transaction> trans = new List<Transaction>
            {
                new Transaction
                {
                    Name = "Old Transaction",
                    Amount = 100,
                    Active = true,
                    StartDate = new DateTime(testYear - 1, 5, 10),
                    Frequency = Frequency.Monthly
                }
            };
            mockTransactionRepository.Setup(x => x.GetAll()).Returns(trans);

            // Act
            var result = await sut.GenerateAll(null);

            // Assert
            result.StatusCode.Should().Be(StatusCode.NotFound);
            result.Message.Should().Contain("No transactions exist which start in the current year");
            result.StatusCode.Should().Be(StatusCode.NotFound);
        }

        [TestMethod]
        public async System.Threading.Tasks.Task GenerateAll_WithMultipleMonthlyTransactions_ReturnsAllPlanDates()
        {
            // Arrange
            var sut = NewDtpService();
            const int testYear = 2024;
            mockDateTimeProvider.Setup(x => x.GetToday()).Returns(new DateTime(testYear, 6, 15));

            Transaction t = new(){
                Frequency = Frequency.Monthly,
                StartDate = new DateTime(testYear, 1, 1)
            };
            IEnumerable<Transaction> trans = new List<Transaction> { t, t, t };
            mockTransactionRepository.Setup(x => x.GetAll()).Returns(trans);

            // Act
            var result = await sut.GenerateAll(null);

            // Assert
            result.StatusCode.Should().Be(StatusCode.Success);
            mockGenerationStrategy.Verify(x => x.Generate(null, It.IsAny<Frequency>()), Times.Exactly(5));
        }
    }
}
