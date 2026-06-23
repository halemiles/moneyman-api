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

namespace Moneyman.Tests
{
    [TestClass]
    public class OffsetCalculationServiceTests
    {
        private Mock<IHolidayService> mockHolidayService;

        private OffsetCalculationService NewOffsetCalculationService() =>
            new OffsetCalculationService(
                mockHolidayService.Object
            );

        [TestInitialize]
        public void SetUp()
        {
            mockHolidayService = new Mock<IHolidayService>();

            var holidays = new List<string>
            {
                "01-01-2022", //New years day
                "05-05-2022", //A random Thursday - Not likley to ever happen IRL
                "20-06-2022", //Bank holiday Monday
                "24-12-2022", //Christmas day
                "25-12-2022"  //Boxing day
            };
            mockHolidayService.Setup(x => x.GenerateHolidays()).Returns(holidays);
        }

        [TestMethod]
        [DataRow("2022,6,18")]
        [DataRow("2022,6,19")]
        [DataRow("2022,6,20")]
        public void CalculateOffset_WhenDateIsWeekendOrBankHoliday_ReturnsOffsetDate(string testValue)
        {
            // Arrange
            var sut = NewOffsetCalculationService();
            var originalDate = DateTime.Parse(testValue);
            var expectedDate = new DateTime(2022,6,21);
            // Act
            var result = sut.CalculateOffset(originalDate);

            // Assert
            result.OriginalPlanDate.Should().Be(originalDate);
            result.PlanDate.Should().Be(expectedDate);
        }

        [TestMethod]
        public void CalculateOffset_WhenCurrentMonthIsDecember_GeneratesNextYear()
        {
            // Arrange
            var sut = NewOffsetCalculationService();
            var originalDate = new DateTime(2023,12,1);

            // Act
            var result = sut.CalculateOffset(originalDate);

            // Assert
            result.PlanDate.Year.Should().Be(2023);
        }

        [TestMethod]
        public void CalculateOffset_WhenNoWorkingDayFoundWithinLimit_StopsAndReturnsInvalid()
        {
            // Arrange — mark a contiguous run of days as bank holidays so a valid
            // working day is never found. The loop guard must cap the search at 10
            // iterations rather than spin forever, leaving the result invalid.
            var contiguousHolidays = Enumerable.Range(0, 12)
                .Select(offset => new DateTime(2022, 3, 1).AddDays(offset).ToString("dd-MM-yyyy"))
                .ToList();
            mockHolidayService.Setup(x => x.GenerateHolidays()).Returns(contiguousHolidays);
            var sut = NewOffsetCalculationService();

            // Act
            var result = sut.CalculateOffset(new DateTime(2022, 3, 1));

            // Assert
            result.IsValid.Should().BeFalse();
            result.OffsetBy.Should().Be(11);
        }
    }
}
