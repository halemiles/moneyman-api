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
    public class DteObjectTests //TODO - Rename this
    {
        private Mock<ITransactionService> mockTransactionService;
        private Mock<ITransactionRepository> mockTransactionRepository;
        private Mock<IHolidayService> mockHolidayService;

        private OffsetCalculationService NewOffsetCalculationService() =>
            new OffsetCalculationService(
                new WeekdayService(),
                mockHolidayService.Object
            );

        [TestInitialize]
        public void SetUp()
        {
            mockTransactionService = new Mock<ITransactionService>();
            mockTransactionRepository = new Mock<ITransactionRepository>();
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
        public void PlanDateString_WithDate_ReturnsStringInCorrectFormat()
        {
            // Arrange
            var sut = new DteObject()
            {
                PlanDate = new DateTime(2022,10,20)
            };
            // Act
            var result = sut.PlanDateString;

            // Assert
            result.Should().Be("20-10-2022");
        }
    }
}
