using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moneyman.Interfaces;
using Moneyman.Services;
using Moneyman.Domain;
using FluentAssertions;
using System.Collections.Generic;
using System;

namespace Moneyman.Tests
{
    [TestClass]
    public class CalculateOffsetServiceTests
    {
        private Mock<IHolidayService> mockHolidayService;

        private OffsetCalculationService NewSut() =>
            new OffsetCalculationService(mockHolidayService.Object);

        // Holidays used across tests:
        //   2024-03-29  Good Friday
        //   2024-04-01  Easter Monday
        //   2024-12-25  Christmas Day (Wednesday)
        //   2024-12-26  Boxing Day    (Thursday)
        private static List<string> StandardHolidays() => new List<string>
        {
            "29-03-2024", // Good Friday
            "01-04-2024", // Easter Monday
            "25-12-2024", // Christmas Day (Wednesday)
            "26-12-2024"  // Boxing Day    (Thursday)
        };

        [TestInitialize]
        public void SetUp()
        {
            mockHolidayService = new Mock<IHolidayService>();
            mockHolidayService.Setup(x => x.GenerateHolidays()).Returns(StandardHolidays());
        }

        // ── Row 1: due date is a normal weekday ────────────────────────────────

        [TestMethod]
        [DataRow(2024, 4, 3)]  // Wednesday (after Easter)
        [DataRow(2024, 4, 4)]  // Thursday
        [DataRow(2024, 4, 2)]  // Tuesday
        public void CalculateOffset_WhenDueDateIsNormalWeekday_ReturnsSameDate(int y, int m, int d)
        {
            var due = new DateTime(y, m, d);
            var result = NewSut().CalculateOffset(due);

            result.PlanDate.Should().Be(due);
            result.OffsetBy.Should().Be(0);
        }

        // ── Row 2: due date is a Saturday → following Monday ──────────────────

        [TestMethod]
        public void CalculateOffset_WhenDueDateIsSaturday_ReturnsFollowingMonday()
        {
            var saturday = new DateTime(2024, 4, 6);    // Saturday
            var expectedMonday = new DateTime(2024, 4, 8); // Monday (not a holiday)

            var result = NewSut().CalculateOffset(saturday);

            result.PlanDate.Should().Be(expectedMonday);
            result.OffsetBy.Should().Be(2);
        }

        // ── Row 3: due date is a Sunday → following Monday ────────────────────

        [TestMethod]
        public void CalculateOffset_WhenDueDateIsSunday_ReturnsFollowingMonday()
        {
            var sunday = new DateTime(2024, 4, 7);      // Sunday
            var expectedMonday = new DateTime(2024, 4, 8); // Monday (not a holiday)

            var result = NewSut().CalculateOffset(sunday);

            result.PlanDate.Should().Be(expectedMonday);
            result.OffsetBy.Should().Be(1);
        }

        // ── Row 4: due date is a Monday bank holiday → following Tuesday ───────

        [TestMethod]
        public void CalculateOffset_WhenDueDateIsEasterMonday_ReturnsFollowingTuesday()
        {
            var easterMonday = new DateTime(2024, 4, 1);  // bank holiday Monday
            var expectedTuesday = new DateTime(2024, 4, 2);

            var result = NewSut().CalculateOffset(easterMonday);

            result.PlanDate.Should().Be(expectedTuesday);
            result.OffsetBy.Should().Be(1);
        }

        // ── Row 5: due date is a Friday bank holiday → following Tuesday ──────
        // Good Friday is always followed by Easter Monday (also a bank holiday),
        // so the next banking day is Tuesday, not Monday.

        [TestMethod]
        public void CalculateOffset_WhenDueDateIsGoodFriday_ReturnsFollowingTuesday()
        {
            // Good Friday 29-Mar-2024 → Easter Monday 01-Apr is also a holiday
            // so the next banking day is Tuesday 02-Apr
            var goodFriday = new DateTime(2024, 3, 29);
            var expectedTuesday = new DateTime(2024, 4, 2);

            var result = NewSut().CalculateOffset(goodFriday);

            result.PlanDate.Should().Be(expectedTuesday);
            result.OffsetBy.Should().Be(4);
        }

        // ── Row 6: Saturday, following Monday is a bank holiday → Tuesday ──────

        [TestMethod]
        public void CalculateOffset_WhenSaturdayAndFollowingMondayIsBankHoliday_ReturnsFollowingTuesday()
        {
            // Saturday 30-Mar-2024 → Sunday 31 → Monday 01-Apr (Easter Monday BH) → Tuesday 02-Apr
            var saturday = new DateTime(2024, 3, 30);
            var expectedTuesday = new DateTime(2024, 4, 2);

            var result = NewSut().CalculateOffset(saturday);

            result.PlanDate.Should().Be(expectedTuesday);
            result.OffsetBy.Should().Be(3);
        }

        // ── Row 7: Sunday, following Monday is a bank holiday → Tuesday ────────

        [TestMethod]
        public void CalculateOffset_WhenSundayAndFollowingMondayIsBankHoliday_ReturnsFollowingTuesday()
        {
            // Easter Sunday 31-Mar-2024 → Monday 01-Apr (Easter Monday BH) → Tuesday 02-Apr
            var easterSunday = new DateTime(2024, 3, 31);
            var expectedTuesday = new DateTime(2024, 4, 2);

            var result = NewSut().CalculateOffset(easterSunday);

            result.PlanDate.Should().Be(expectedTuesday);
            result.OffsetBy.Should().Be(2);
        }

        // ── Row 8: consecutive bank holidays → next clear weekday ─────────────

        [TestMethod]
        public void CalculateOffset_WhenConsecutiveBankHolidays_ReturnsNextClearWeekday()
        {
            // Christmas Wed 25-Dec-2024 (BH) → Boxing Day Thu 26-Dec-2024 (BH) → Friday 27-Dec-2024 (clear)
            var christmasDay = new DateTime(2024, 12, 25);
            var expectedFriday = new DateTime(2024, 12, 27);

            var result = NewSut().CalculateOffset(christmasDay);

            result.PlanDate.Should().Be(expectedFriday);
            result.OffsetBy.Should().Be(2);
        }

        // ── Reason field is set when an offset occurs ──────────────────────────

        [TestMethod]
        public void CalculateOffset_WhenOffsetRequired_ReasonIsPopulated()
        {
            var saturday = new DateTime(2024, 4, 6);
            var result = NewSut().CalculateOffset(saturday);

            result.Reason.Should().NotBeNullOrEmpty();
        }

        [TestMethod]
        public void CalculateOffset_WhenNoOffsetRequired_OriginalPlanDateIsPreserved()
        {
            var wednesday = new DateTime(2024, 4, 3);
            var result = NewSut().CalculateOffset(wednesday);

            result.OriginalPlanDate.Should().Be(wednesday);
            result.PlanDate.Should().Be(wednesday);
        }
    }
}
