using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moneyman.Interfaces;
using Moneyman.Services;
using Moneyman.Domain;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;

namespace Moneyman.Tests
{
    [TestClass]
    public class DtpMonthlyGenerationTests
    {
        private Mock<ITransactionService> mockTransactionService;
        private Mock<ITransactionRepository> mockTransactionRepository;

        private DtpService NewDtpGenerationService() =>
            new DtpService(mockTransactionRepository.Object);

        [TestInitialize]
        public void SetUp()
        {
            mockTransactionService = new Mock<ITransactionService>();
            mockTransactionRepository = new Mock<ITransactionRepository>();
        }

        [TestMethod]
        public void GenerateMonthly_WithInvalidTransactionId_ReturnsEmptyList()
        {
            // Arrange
            var sut = NewDtpGenerationService();
            IEnumerable<Transaction> trans = new List<Transaction>();
            mockTransactionRepository.Setup(x => x.GetAll()).Returns(trans);

            // Act
            var result = sut.GenerateMonthly(0);

            // Assert
            result.Count.Should().Be(0);
        }

        [TestMethod]
        public void GenerateMonthly_WithValidMonthlyTransaction_ReturnsSuccess()
        {
            // Arrange
            var sut = NewDtpGenerationService();
            IEnumerable<Transaction> trans = new List<Transaction>()
            {
                new Transaction(){
                    Name = "Trans 1",
                    Amount = 100,
                    Active = true
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
