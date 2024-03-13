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

namespace Moneyman.Tests
{
    [TestClass]
    public class DtpMonthlyGenerationIntegrationTests
    {
        private Mock<ITransactionService> mockTransactionService;
        private Mock<ITransactionRepository> mockTransactionRepository;
        private Mock<IPlanDateRepository> mockPlanDateRepository;
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

        private Mock<IHolidayService> mockHolidayService = new Mock<IHolidayService>();

        //TODO - Move this to a fixture class
        private OffsetCalculationService NewOffsetCalculationService() =>
            new(
                new WeekdayService(),
                mockHolidayService.Object
            );

        private DtpService NewDtpGenerationService() =>
            new DtpService(
                    mockTransactionRepository.Object,
                    mockPlanDateRepository.Object,
                    NewOffsetCalculationService(),
                    mockPaydayService.Object,
                    mockLogger.Object
            );

        [TestInitialize]
        public void SetUp()
        {
            mockTransactionService = new Mock<ITransactionService>();
            mockTransactionRepository = new Mock<ITransactionRepository>();
            mockPlanDateRepository = new Mock<IPlanDateRepository>();
            var mockOffsetCalculationService = new Mock<IOffsetCalculationService>();
            mockPaydayService = new Mock<IPaydayService>();
            mockLogger = new Mock<ILogger<DtpService>>();

            mockHolidayService = new Mock<IHolidayService>();

            mockOffsetCalculationService.Setup(x => x.CalculateOffset(It.IsAny<DateTime>()))
                .Returns(new CalculatedPlanDate());

            mockHolidayService.Setup(x => x.GenerateHolidays()).Returns(holidays);
            mockPaydayService.Setup(x => x.GetAll()).Returns(new List<Payday>());
        }

        [TestMethod]
        [DataRow("2022-01-08",10,8,8,8,9,8,8,8,8,10,8,8)]
        [DataRow("2022-01-15",17,15,15,19,16,15,15,15,15,17,15,15)] //Includes two bank holidays in April
        [DataRow("2022-05-08",10,8,8,8,9,8,8,8,8,10,8,8)]
        public void GenerateMonthly_WithExpectedValues_ReturnsSuccess(string startDateString,
            int day1,int day2,int day3,int day4,int day5,int day6,int day7,int day8,int day9,int day10,int day11,int day12
        )
        {
            // Arrange
            List<int> expectedDayValues = new() {day1,day2,day3,day4,day5,day6,day7,day8,day9,day10,day11,day12};
            var startDate = DateTime.Parse(startDateString);
            var sut = NewDtpGenerationService();
            
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
            results.Count.Should().Be(12);
            for(int resultCounter = 0; resultCounter < results.Count; resultCounter++)
            {
                results[resultCounter].Date.Day.Should().Be(expectedDayValues[resultCounter]);
                results[resultCounter].Date.Month.Should().Be(resultCounter+1);
            }
        }
    }
}
