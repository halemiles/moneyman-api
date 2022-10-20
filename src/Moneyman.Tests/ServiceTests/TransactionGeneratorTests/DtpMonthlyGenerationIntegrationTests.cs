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

namespace Moneyman.Tests
{
    [TestClass]
    public class DtpMonthlyGenerationIntegrationTests
    {
        private Mock<ITransactionService> mockTransactionService;
        private Mock<ITransactionRepository> mockTransactionRepository;
        private Mock<IPlanDateRepository> mockPlanDateRepository;
        private Mock<IOffsetCalculationService> mockOffsetCalculationService;

        //Mocking offset calculation service
        private Mock<IHolidayService> mockHolidayService;

        private List<string> holidays = new List<string>(){
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

        private OffsetCalculationService NewOffsetCalculationService() =>
            new(
                new WeekdayService(),
                mockHolidayService.Object
            );

        private DtpService NewDtpGenerationService() =>
            new DtpService(
                    mockTransactionRepository.Object,
                    mockPlanDateRepository.Object,
                    NewOffsetCalculationService()
            );

        [TestInitialize]
        public void SetUp()
        {
            mockTransactionService = new Mock<ITransactionService>();
            mockTransactionRepository = new Mock<ITransactionRepository>();
            mockPlanDateRepository = new Mock<IPlanDateRepository>();
            mockOffsetCalculationService = new Mock<IOffsetCalculationService>();

            mockHolidayService = new Mock<IHolidayService>();

            mockOffsetCalculationService.Setup(x => x.CalculateOffset(It.IsAny<DateTime>()))
                .Returns(new DteObject());

            mockHolidayService.Setup(x => x.GenerateHolidays()).Returns(holidays);
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
            var fixture = new Fixture().Customize(new AutoMoqCustomization());

            IEnumerable<Transaction> transactions = new List<Transaction>()
            {
                fixture.Build<Transaction>().With(f => f.StartDate, startDate).Create()
            }.AsEnumerable();

            mockTransactionRepository.Setup(x => x.GetAll()).Returns(transactions);

            // Act
            var results = sut.GenerateMonthly(0);

            // Assert
            results.Count.Should().Be(12);
            for(int resultCounter = 0; resultCounter < results.Count; resultCounter++)
            {
                 results[resultCounter].Date.Should().Be( DateTime.Now.WithMonth((resultCounter+1)).WithDate(expectedDayValues[resultCounter]));
            }
        }
    }
}
