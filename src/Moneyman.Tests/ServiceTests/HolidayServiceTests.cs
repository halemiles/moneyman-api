using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moneyman.Domain;
using Moneyman.Interfaces;
using Moneyman.Services;

namespace Moneyman.Tests
{
    [TestClass]
    public class HolidayServiceTests
    {
        [TestMethod]
        public void GenerateHolidays_WhenCacheContainsValues_ReturnsCacheValues()
        {
            var cacheValues = new List<string> { "01-01-2026" };
            var cache = new Mock<IBankHolidayCache>();
            cache.SetupGet(x => x.Holidays).Returns(cacheValues);

            var repository = new Mock<IBankHolidayRepository>();

            var sut = new HolidayService(cache.Object, repository.Object);
            var result = sut.GenerateHolidays();

            result.Should().BeEquivalentTo(cacheValues);
            repository.Verify(x => x.GetAll(), Times.Never);
        }

        [TestMethod]
        public void GenerateHolidays_WhenCacheEmptyAndRepositoryHasData_ReturnsRepositoryValuesAndPopulatesCache()
        {
            var cache = new Mock<IBankHolidayCache>();
            cache.SetupGet(x => x.Holidays).Returns(new List<string>());

            var repositoryValues = new List<BankHoliday>
            {
                new BankHoliday { Date = new DateTime(2026, 1, 1), Title = "New Year's Day" }
            };

            var repository = new Mock<IBankHolidayRepository>();
            repository.Setup(x => x.GetAll()).Returns(repositoryValues);

            var sut = new HolidayService(cache.Object, repository.Object);

            var result = sut.GenerateHolidays();

            result.Should().BeEquivalentTo(new List<string> { "01-01-2026" });
            cache.Verify(x => x.Populate(It.Is<IEnumerable<string>>(h => HasSingleValue(h, "01-01-2026"))), Times.Once);
        }

        [TestMethod]
        public void GenerateHolidays_WhenCacheAndRepositoryEmpty_ReturnsEmptyList()
        {
            var cache = new Mock<IBankHolidayCache>();
            cache.SetupGet(x => x.Holidays).Returns(new List<string>());

            var repository = new Mock<IBankHolidayRepository>();
            repository.Setup(x => x.GetAll()).Returns(new List<BankHoliday>());

            var sut = new HolidayService(cache.Object, repository.Object);

            var result = sut.GenerateHolidays();

            result.Should().BeEquivalentTo(new List<string>());
            cache.Verify(x => x.Populate(It.IsAny<IEnumerable<string>>()), Times.Never);
        }

        private static bool HasSingleValue(IEnumerable<string> values, string expected)
        {
            var list = new List<string>(values);
            return list.Count == 1 && list[0] == expected;
        }
    }
}
