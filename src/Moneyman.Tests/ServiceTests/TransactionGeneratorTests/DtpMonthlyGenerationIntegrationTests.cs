using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moneyman.Interfaces;
using Moneyman.Services;
using Moneyman.Domain;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using System;
using AutoFixture;
using AutoFixture.AutoMoq;
using Moneyman.Tests.Extensions;
using Snapper;
using Snapper.Core;
using Microsoft.Extensions.Logging;
using Moneyman.Services.Interfaces;

using Moneyman.Domain.MapperProfiles;

namespace Moneyman.Tests
{
    [TestClass]
    public class DtpMonthlyGenerationIntegrationTests
    {
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

        private Mock<IHolidayService> mockHolidayService = new Mock<IHolidayService>();

        //TODO - Move this to a fixture class
        private OffsetCalculationService NewOffsetCalculationService() =>
            new(
                new WeekdayService(),
                mockHolidayService.Object
            );

        private Mock<ITransactionRepository> mockTransactionRepository;
        private Mock<IPlanDateRepository> mockPlanDateRepository;
        private Mock<IOffsetCalculationService> mockOffsetCalculationService;
        private Mock<IPaydayService> mockPaydayService;
        private Mock<IDateTimeProvider> mockDateTimeProvider;
        private Mock<ILogger<DtpService>> mockLogger;
        private Mock<PlanDateMapper> mockPlanDateMapper;

        private DtpService NewDtpService() =>
            new DtpService(
                    mockTransactionRepository.Object,
                    mockPlanDateRepository.Object,
                    mockOffsetCalculationService.Object,
                    mockPaydayService.Object,
                    mockDateTimeProvider.Object,
                    mockLogger.Object,
                    mockPlanDateMapper.Object
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
            mockPlanDateMapper = new Mock<PlanDateMapper>();

            mockHolidayService = new Mock<IHolidayService>();

            mockOffsetCalculationService.Setup(x => x.CalculateOffset(It.IsAny<DateTime>()))
                .Returns(new CalculatedPlanDate());

            mockHolidayService.Setup(x => x.GenerateHolidays()).Returns(holidays);
            mockPaydayService.Setup(x => x.GetAll()).Returns(new List<Payday>());
        }

        [TestMethod]
        [DataRow("2022-01-08",10,8,8,8,9,8,8,8,8,10,8,8)]
        public void GenerateMonthly_WithExpectedValues_ReturnsSuccess_1(string startDateString,
            int day1,int day2,int day3,int day4,int day5,int day6,int day7,int day8,int day9,int day10,int day11,int day12
        )
        {
            // Arrange
            var startDate = DateTime.Parse(startDateString);
            var sut = NewDtpService();

            IEnumerable<Transaction> transactions = new List<Transaction>
            {
                new Transaction
                {
                    Id = 0,
                    Name = "transaction 1",
                    StartDate = startDate,
                    Frequency = Frequency.Monthly
                }
            }.AsEnumerable();

            mockTransactionRepository.Setup(x => x.GetAll()).Returns(transactions);

            // Act
            var results = sut.GenerateMonthly(0);

            // Assert
            results.Count.Should().Be(24);
            results.ShouldMatchSnapshot();
        }

        [TestMethod]
        [DataRow("2022-01-15",17,15,15,19,16,15,15,15,15,17,15,15)] //Includes two bank holidays in April
        public void GenerateMonthly_WithExpectedValues_ReturnsSuccess_2(string startDateString,
            int day1,int day2,int day3,int day4,int day5,int day6,int day7,int day8,int day9,int day10,int day11,int day12
        )
        {
            // Arrange
            var startDate = DateTime.Parse(startDateString);
            var sut = NewDtpService();

            IEnumerable<Transaction> transactions = new List<Transaction>
            {
                new Transaction
                {
                    Id = 0,
                    Name = "transaction 1",
                    StartDate = startDate,
                    Frequency = Frequency.Monthly
                }
            }.AsEnumerable();

            mockTransactionRepository.Setup(x => x.GetAll()).Returns(transactions);

            // Act
            var results = sut.GenerateMonthly(0);

            // Assert
            results.Count.Should().Be(24);
            results.ShouldMatchSnapshot();
        }

        [TestMethod]
        [DataRow("2022-05-08",10,8,8,8,9,8,8,8,8,10,8,8)]
        public void GenerateMonthly_WithExpectedValues_ReturnsSuccess_3(string startDateString,
            int day1,int day2,int day3,int day4,int day5,int day6,int day7,int day8,int day9,int day10,int day11,int day12
        )
        {
            // Arrange
            var startDate = DateTime.Parse(startDateString);
            var sut = NewDtpService();

            IEnumerable<Transaction> transactions = new List<Transaction>
            {
                new Transaction
                {
                    Id = 0,
                    Name = "transaction 1",
                    StartDate = startDate,
                    Frequency = Frequency.Monthly
                }
            }.AsEnumerable();

            mockTransactionRepository.Setup(x => x.GetAll()).Returns(transactions);

            // Act
            var results = sut.GenerateMonthly(0);

            // Assert
            results.Count.Should().Be(24);
            results.ShouldMatchSnapshot();
        }
    }
}
