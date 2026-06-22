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

        private OffsetCalculationService NewOffsetCalculationService() =>
            new(
                mockHolidayService.Object
            );

        private Mock<ITransactionRepository> mockTransactionRepository;
        private Mock<ILogger<DtpService>> mockLogger;
        private DefaultPlanDateGenerationStrategy sut;

        [TestInitialize]
        public void SetUp()
        {
            mockTransactionRepository = new Mock<ITransactionRepository>();
            mockLogger = new Mock<ILogger<DtpService>>();

            mockHolidayService = new Mock<IHolidayService>();
            mockHolidayService.Setup(x => x.GenerateHolidays()).Returns(holidays);

            sut = new DefaultPlanDateGenerationStrategy(
                mockTransactionRepository.Object,
                NewOffsetCalculationService(),
                mockLogger.Object
            );
        }

        // Each start date is a distinct case verified against its own child
        // snapshot. The generated plan dates depend on weekend/bank-holiday
        // offsetting, so the snapshot is the source of truth for expected output.
        [DataTestMethod]
        [DataRow("2022-01-08")]
        [DataRow("2022-01-15")] // Start date whose April recurrence hits two bank holidays
        [DataRow("2022-05-08")]
        public void GenerateMonthly_WithExpectedValues_ReturnsSuccess(string startDateString)
        {
            // Arrange
            var startDate = DateTime.Parse(startDateString);

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
            var results = sut.Generate(null, Frequency.Monthly);

            // Assert
            results.Count.Should().Be(24);
            results.ShouldMatchChildSnapshot(startDateString);
        }
    }
}
