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

        private readonly List<Payday> payDates = new List<Payday>
        {
            new Payday { Date = DateTime.Parse("2022-01-25")},
            new Payday { Date = DateTime.Parse("2022-02-25")},
            new Payday { Date = DateTime.Parse("2022-03-25")},
            new Payday { Date = DateTime.Parse("2022-04-25")},
            new Payday { Date = DateTime.Parse("2022-05-25")},
            new Payday { Date = DateTime.Parse("2022-06-27")},
            new Payday { Date = DateTime.Parse("2022-07-25")},
            new Payday { Date = DateTime.Parse("2022-08-25")},
            new Payday { Date = DateTime.Parse("2022-09-26")},
            new Payday { Date = DateTime.Parse("2022-10-25")},
            new Payday { Date = DateTime.Parse("2022-11-25")},
            new Payday { Date = DateTime.Parse("2022-12-28")}
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

            mockPaydayRepository.Setup(x => x.GetAll())
                .Returns(payDates);

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

        [TestMethod]
        public void GetPrevious_ReturnsSuccess()
        {
            var paydayService = NewPaydayService();
            mockDateTimeProvider.Setup(x => x.GetNow())
                .Returns(new DateTime(2022,6,27));
            var result = paydayService.GetPrevious();

            result.Date.Should().Be(new DateTime(2022,5,25));
        }

        [TestMethod]
        public void GetPrevious_WhenOnPayday_ReturnsSuccess()
        {
            var paydayService = NewPaydayService();
            mockDateTimeProvider.Setup(x => x.GetNow())
                .Returns(new DateTime(2022,6,20));
            var result = paydayService.GetPrevious();

            result.Date.Should().Be(new DateTime(2022,5,25));
        }

        [TestMethod]
        public void GetPrevious_WhenDayAfterPayday_ReturnsSuccess()
        {
            var paydayService = NewPaydayService();
            mockDateTimeProvider.Setup(x => x.GetNow())
                .Returns(new DateTime(2022,6,28));
            var result = paydayService.GetPrevious();

            result.Date.Should().Be(new DateTime(2022,6,27));
        }

               [TestMethod]
        public void GetNext_ReturnsSuccess()
        {
            var paydayService = NewPaydayService();
            mockDateTimeProvider.Setup(x => x.GetNow())
                .Returns(new DateTime(2022,6,20));
            var result = paydayService.GetNext();

            result.Date.Should().Be(new DateTime(2022,6,27));
        }

        [TestMethod]
        public void GetNext_WhenOnPayday_ReturnsSuccess()
        {
            var paydayService = NewPaydayService();
            mockDateTimeProvider.Setup(x => x.GetNow())
                .Returns(new DateTime(2022,5,25));
            var result = paydayService.GetNext();

            result.Date.Should().Be(new DateTime(2022,6,27));
        }

        [TestMethod]
        public void GetNext_WhenDayBeforePayday_ReturnsSuccess()
        {
            var paydayService = NewPaydayService();
            mockDateTimeProvider.Setup(x => x.GetNow())
                .Returns(new DateTime(2022,5,25));
            var result = paydayService.GetNext();

            result.Date.Should().Be(new DateTime(2022,6,27));
        }
    }
}
