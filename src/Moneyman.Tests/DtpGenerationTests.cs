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

namespace Moneyman.Tests
{
    [TestClass]
    public class DtpGenerationTests
    {
        private Mock<ITransactionService> mockTransactionService;
        private Mock<ITransactionRepository> mockTransactionRepository;
        private Mock<IPlanDateRepository> mockPlanDateRepository;
        private Mock<IOffsetCalculationService> mockOffsetCalculationService;
        private Mock<ILogger<DtpService>> mockLogger;

        private DtpService NewDtpGenerationService() =>
            new DtpService(
                mockTransactionRepository.Object,
                mockPlanDateRepository.Object,
                mockOffsetCalculationService.Object,
                mockLogger.Object
            );

        [TestInitialize]
        public void SetUp()
        {
            mockTransactionService = new Mock<ITransactionService>();
            mockTransactionRepository = new Mock<ITransactionRepository>();
            mockPlanDateRepository = new Mock<IPlanDateRepository>();
            mockOffsetCalculationService = new Mock<IOffsetCalculationService>();
            mockLogger = new Mock<ILogger<DtpService>>();

            mockOffsetCalculationService.Setup(x => x.CalculateOffset(It.IsAny<DateTime>()))
                .Returns(new CalculatedPlanDate());
        }

        //TODO - Could this be more generic?
        [TestMethod]
        public void Generate_WithValidMonthlyTransaction_ReturnsSuccess()
        {
            // Arrange
            var sut = NewDtpGenerationService();
            IEnumerable<Transaction> trans = new List<Transaction>()
            {
                new Transaction(){
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
            result.Count.Should().Be(12);
            result.All(x => x.Transaction.Name == "Trans 1").Should().BeTrue();
        }
    }
}
