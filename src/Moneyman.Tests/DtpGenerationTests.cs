using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moneyman.Interfaces;
using Moneyman.Services;
using Moneyman.Domain;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using System;
using Microsoft.Extensions.Logging;
using Moneyman.Services.Interfaces;
using AutoMapper;

namespace Moneyman.Tests
{
    [TestClass]
    public class DtpGenerationTests
    {
        private Mock<ITransactionRepository> mockTransactionRepository;
        private Mock<IPlanDateRepository> mockPlanDateRepository;
        private Mock<IOffsetCalculationService> mockOffsetCalculationService;
        private Mock<IPaydayService> mockPaydayService;
        private Mock<IDateTimeProvider> mockDateTimeProvider;
        private IMapper mockMapper;
        private Mock<ILogger<DtpService>> mockLogger;

        private DtpService NewDtpService() =>
            new DtpService(
                    mockTransactionRepository.Object,
                    mockPlanDateRepository.Object,
                    mockOffsetCalculationService.Object,
                    mockPaydayService.Object,
                    mockMapper,
                    mockDateTimeProvider.Object,
                    mockLogger.Object
            );

        [TestInitialize]
        public void SetUp()
        {
            mockPlanDateRepository = new Mock<IPlanDateRepository>();
            mockTransactionRepository = new Mock<ITransactionRepository>();
            mockOffsetCalculationService = new Mock<IOffsetCalculationService>();
            mockPaydayService = new Mock<IPaydayService>();
            mockDateTimeProvider = new Mock<IDateTimeProvider>();
            mockLogger = new Mock<ILogger<DtpService>>();

            mockOffsetCalculationService.Setup(x => x.CalculateOffset(It.IsAny<DateTime>()))
                .Returns(new CalculatedPlanDate());

            mockPaydayService.Setup(x => x.GetAll()).Returns(new List<Payday>());
        }

        //TODO - Could this be more generic?
        [TestMethod]
        public void Generate_WithValidMonthlyTransaction_ReturnsSuccess()
        {
            // Arrange
            var sut = NewDtpService();
            IEnumerable<Transaction> trans = new List<Transaction>
            {
                new Transaction
                {
                    Name = "Trans 1",
                    Amount = 100,
                    Active = true,
                    StartDate = new DateTime(2022,1,1),
                    Frequency = Frequency.Monthly
                }
            }.AsEnumerable();
            mockTransactionRepository.Setup(x => x.GetAll()).Returns(trans);

            // Act
            var result = sut.GenerateMonthly(0);

            // Assert
            result.Count.Should().Be(24);
            result.All(x => x.Transaction.Name == "Trans 1").Should().BeTrue();
        }
    }
}
