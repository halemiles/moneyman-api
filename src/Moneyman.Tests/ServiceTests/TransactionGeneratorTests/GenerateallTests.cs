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
using AutoFixture;
using Microsoft.Extensions.Logging;
using Moneyman.Services.Interfaces;

namespace Moneyman.Tests
{
    [TestClass]
    public class GenerateAllTests
    {
        private Mock<ITransactionService> mockTransactionService;
        private Mock<ITransactionRepository> mockTransactionRepository;
        private Mock<IPlanDateRepository> mockPlanDateRepository;
        private Mock<IOffsetCalculationService> mockOffsetCalculationService;
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

        private DtpService NewDtpGenerationService() =>
            new DtpService(
                    mockTransactionRepository.Object,
                    mockPlanDateRepository.Object,
                    mockOffsetCalculationService.Object,
                    mockPaydayService.Object,
                    mockLogger.Object
            );

        [TestInitialize]
        public void SetUp()
        {
            mockTransactionService = new Mock<ITransactionService>();
            mockTransactionRepository = new Mock<ITransactionRepository>();
            mockPlanDateRepository = new Mock<IPlanDateRepository>();
            mockOffsetCalculationService = new Mock<IOffsetCalculationService>();
            mockPaydayService = new Mock<IPaydayService>();
            mockLogger = new Mock<ILogger<DtpService>>();

            mockOffsetCalculationService.Setup(x => x.CalculateOffset(It.IsAny<DateTime>()))
                .Returns(new CalculatedPlanDate());

            mockPaydayService.Setup(x => x.GetAll()).Returns(new List<Payday>(){new Payday()});
        }

        [TestMethod]
        public void GenerateMonthly_WhenNoPaydaysExist_ReturnsNotFound()
        {
            // Arrange
            var sut = NewDtpGenerationService();
            mockPaydayService.Setup(x => x.GetAll()).Returns(new List<Payday>());
            // Act
            var result = sut.GenerateAll(null);

            // Assert
            result.StatusCode.Should().Be(Domain.Models.StatusCode.NotFound);
        }

        [TestMethod]
        public void GenerateMonthly_WithInvalidTransactionId_ReturnsEmptyList()
        {
            // Arrange
            var sut = NewDtpGenerationService();
            Fixture fixture = new Fixture();
            IEnumerable<Transaction> trans = new List<Transaction>
            {
                fixture.Create<Transaction>(),
                fixture.Create<Transaction>(),
                fixture.Create<Transaction>()
            };
            mockTransactionRepository.Setup(x => x.GetAll()).Returns(trans);
            mockPlanDateRepository.Setup(x => x.GetAll()).Returns(new List<PlanDate>(){new PlanDate()});
            // Act
            var result = sut.GenerateAll(null);

            // Assert
            result.StatusCode.Should().Be(Domain.Models.StatusCode.Success);
            result.Payload.Count().Should().Be(0);
        }
    }
}
