using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moneyman.Interfaces;
using Moneyman.Services;
using Moneyman.Domain;
using FluentAssertions;
using System.Threading.Tasks;
using FluentValidation;
using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Moneyman.Tests
{
    [TestClass]
    public class TransactionServiceTests
    {
        private Mock<ITransactionService> mockTransactionService;
        private Mock<ITransactionRepository> mockTransactionRepository;
        private Mock<IValidator<Transaction>> mockTransactionValidator;
        TransactionService NewTransactionService() => 
            new TransactionService(mockTransactionRepository.Object);

        [TestInitialize]
        public void SetUp()
        {
            mockTransactionRepository = new Mock<ITransactionRepository>();
            mockTransactionService = new Mock<ITransactionService>();
            mockTransactionValidator = new Mock<IValidator<Transaction>>();
            mockTransactionRepository.Setup(x => x.Save()).Returns(Task.FromResult(1));

        }

        [TestMethod]
        public void Create_WithValidDetails_ReturnsSuccess()
        {
            var transactionService = NewTransactionService();
            Transaction trans = new Transaction
            {
                Id = 999,
                Name = "Trans 1",
                Date = new DateTime(2021,1,1),
                Amount = 100,
                Active = true
            };

            var result = transactionService.Create(trans);
            mockTransactionValidator.Verify(x => x.Validate(trans),Times.Once);
            mockLogger.Verify(x => x.LogInformation("Validation transaction {TransactionName}", trans.Name), Times.Once);
            result.Should().Be(true);
        }

        [TestMethod]
        public void Create_WithInvalidDetails_ReturnsFailure()
        {
            var transactionService = NewTransactionService();
            Transaction trans = new Transaction
            {
                Id = 999,
                Name = null,
                Amount = 100,
                Active = true
            };

            var result = transactionService.Create(trans);
            result.Should().Be(false);
        }

        [TestMethod]
        public void Update_WithValidDetails_ReturnsSuccess()
        {
            var transactionService = NewTransactionService();
            mockTransactionRepository.Setup(x => x.Update(It.IsAny<Transaction>())).Returns(true);
            
            Transaction trans = new Transaction
            {
                Id = 999,
                Name = "Trans 1",
                Amount = 100,
                Active = true
            };

            var result = transactionService.Update(trans);
            result.Should().Be(999);
        }

        [TestMethod]
        public void GetAll_WithValidDetails_ReturnsSuccess()
        {
            var transactionService = NewTransactionService();
            List<Transaction> transactions = new List<Transaction>
            {
                new Transaction()
            };
            mockTransactionRepository.Setup(x => x.GetAll()).Returns(transactions);
            
            var result = transactionService.GetAll();
            result.Count.Should().Be(1);
        }

        [TestMethod]
        public void GetById_WithValidDetails_ReturnsSuccess()
        {
            var transactionService = NewTransactionService();
            var sutTransaction = new Transaction{Id = 1};
            mockTransactionRepository.Setup(x => x.Get(It.IsAny<int>())).Returns(sutTransaction);
            
            var result = transactionService.GetById(1);
            result.Id.Should().Be(sutTransaction.Id);
        }
    }
}
