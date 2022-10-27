using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moneyman.Interfaces;
using Moneyman.Services;
using AutoMapper;
using System.Linq;
using FluentAssertions;
using Snapper;
using System.Collections.Generic;
using Moneyman.Domain;
using System;

namespace Moneyman.Tests
{
    [TestClass]
    public class PaydayServiceTests
    {
        private Mock<IPaydayRepository> mockPaydayRepository;
        private Mock<IOffsetCalculationService> mockOffsetCalculationService;
        private Mock<IDateTimeProvider> mockDateTimeProvider;

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
        
        public Mock<IHolidayService> mockHolidayService = new Mock<IHolidayService>();

        //TODO - Move this to a fixture class
        public OffsetCalculationService NewOffsetCalculationService() =>
            new(
                new WeekdayService(),
                mockHolidayService.Object
            );

        private PaydayService NewPaydayService() 
            => new PaydayService(
                mockPaydayRepository.Object,
                NewOffsetCalculationService(),
                mockDateTimeProvider.Object
            );

        [TestInitialize]
        public void SetUp()
        {
            mockPaydayRepository = new Mock<IPaydayRepository>();
            mockOffsetCalculationService = new Mock<IOffsetCalculationService>();
            mockDateTimeProvider = new Mock<IDateTimeProvider>();

            mockOffsetCalculationService.Setup(x => x.CalculateOffset(It.IsAny<DateTime>()))
                .Returns(new DteObject());

            mockHolidayService.Setup(x => x.GenerateHolidays()).Returns(holidays);
        }

        [TestMethod]
        public void Create_WithValidDetails_MatchesSnapshot_ReturnsSuccess()
        {
            int dayOfMonth = 25;
            var paydayService = NewPaydayService();
            var result = paydayService.Generate(dayOfMonth);

            result.ShouldMatchSnapshot();
        }

        [TestMethod]
        public void Create_WithValidDetails_ReturnsSuccess()
        {
            int dayOfMonth = 25;
            var paydayService = NewPaydayService();
            var result = paydayService.Generate(dayOfMonth);

            result.Count.Should().Be(12);
            result.FirstOrDefault().Date.Month.Should().Be(1);
            result.LastOrDefault().Date.Month.Should().Be(12);
        }
    }
}
