using Moneyman.Interfaces;
using Moneyman.Services;
using Moq;
using Moneyman.Domain;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using MockQueryable.Moq;
using Moneyman.Persistence;
using Moneyman.Tests.Builders;
using System;
using System.Threading.Tasks;
using Moneyman.Domain.MapperProfiles;
using Snapper;
using Microsoft.Extensions.Logging;
using AutoFixture;
using System.Runtime.CompilerServices;

namespace Moneyman.Tests
{
    [TestClass]
    public class TransactionServiceTests
    {
        private Mock<ITransactionRepository> _transRepoMock;
        private Mock<IPlanDateRepository> _planDateRepoMock;
        private Mock<ILogger<TransactionService>> mockLogger;
        private Mock<TransactionMapper> _mapper;
        private TransactionService NewTransactionService() =>
            new TransactionService(
                _transRepoMock.Object,
                _planDateRepoMock.Object,
                mockLogger.Object,
                _mapper.Object
            );

        [TestInitialize]
        public void SetUp()
        {
            _transRepoMock = new Mock<ITransactionRepository>();
            _planDateRepoMock = new Mock<IPlanDateRepository>();
            mockLogger = new Mock<ILogger<TransactionService>>();
            _mapper = new Mock<TransactionMapper>();
            
        }

        [TestMethod]
        public void GetAll_WhenNoResults_ReturnsEmptyList()
        {
            _transRepoMock.Setup(x => x.GetAll()).Returns(new List<Transaction>());

            var service = NewTransactionService();
            var result = service.GetAll();
            result.Count().Should().Be(0);
        }

        [TestMethod]
        public void GetById_WhenNoResults_ReturnsEmptyList()
        {
            _transRepoMock.Setup(x => x.Get(It.IsAny<int>())).Returns(new Transaction());

            var service = NewTransactionService();
            var result = service.GetById(0);
            result.Should().NotBeNull();
        }

        [TestMethod]
        public void Update_WhenObjectDoesntExist_ReturnsSuccess()
        {
            var newTransaction = new Transaction
            {
                Name = "newTransaction",
                StartDate = new DateTime(2022,1,1),
                Amount = 150,
                Frequency = Frequency.Weekly
            };
            _transRepoMock.Setup(x => x.Get(It.IsAny<int>()))
                .Returns(newTransaction);
            _transRepoMock.Setup(x => x.Update(It.IsAny<Transaction>()))
                .Returns(true);

            var service = NewTransactionService();
            var result = service.Update(newTransaction);

            _transRepoMock.Verify(x => x.Update(It.IsAny<Transaction>()), Times.Once());
            _transRepoMock.Verify(x => x.Save(), Times.Once());
            result.Should().Be(1);
        }

        [TestMethod]
        public async Task Create_WhenObjectDoesntExist_ReturnsSuccess()
        {
            var newTransaction = new TransactionDto
            {
                Name = "newTransaction",
                StartDate = new DateTime(2022,1,1),
                Amount = 150,
                Frequency = Frequency.Weekly
            };
            var service = NewTransactionService();
            _ = await service.Create(newTransaction);

            _transRepoMock.Verify(x => x.Add(It.IsAny<Transaction>()), Times.Once());
            _transRepoMock.Verify(x => x.Save(), Times.Once());
        }

        [TestMethod]
        [DataRow(null, 100, "2022-01-01")]
        [DataRow("", 100, "2022-01-01")]
        [DataRow("TransactionName", 0, "2022-01-01")]
        [DataRow("TransactionName", 100, "1/1/0001 12:00:00 AM")]
        public async Task Create_WhenObjectDoesntExist_ReturnsFailure(
            string transactionName,
            int amount,
            string startDate
        )
        {
            var newTransaction = new TransactionDto
            {
                Name = transactionName,
                StartDate = DateTime.Parse(startDate),
                Amount = amount,
                Frequency = Frequency.Weekly
            };


            var service = NewTransactionService();
            var result = await service.Create(newTransaction);

            _transRepoMock.Verify(x => x.Add(It.IsAny<Transaction>()), Times.Never());
            _transRepoMock.Verify(x => x.Save(), Times.Never());
            result.Payload.Should().Be(default);
        }

        [TestMethod]
        public void Delete_WhenObjectDoesntExist_ReturnsSuccess()
        {

            var service = NewTransactionService();
            service.Delete(0);

            _transRepoMock.Verify(x => x.Remove(It.IsAny<int>()), Times.Once());
            _transRepoMock.Verify(x => x.Save(), Times.Once());
        }

        [TestMethod]
        public void Update_Multiple_WithMultipleTransactions_ReturnsSuccess()
        {
            var fixture = new Fixture();
            var transactionForUpdate = fixture.Create<Transaction>();
            var newTransactions = new List<Transaction>
            {
                transactionForUpdate,
                fixture.Create<Transaction>(),
                fixture.Create<Transaction>(),
                fixture.Create<Transaction>()
            };

            _transRepoMock.Setup(x => x.Update(It.IsAny<Transaction>()))
                .Returns(true);

            var service = NewTransactionService();
            service.Update(newTransactions);

            _transRepoMock.Verify(x => x.Update(It.IsAny<Transaction>()), Times.Exactly(4));
            _transRepoMock.Verify(x => x.Update(transactionForUpdate), Times.Once());
            _transRepoMock.Verify(x => x.Save(), Times.Once());
        }

        [TestMethod]
        public void Update_WithNullResults_ReturnsEmptyList()
        {
            _transRepoMock.Setup(x => x.Get(1)).Returns(new Transaction
            {
                Id = 1,
                Name = "Transaction Test",
                StartDate = new DateTime(2025,1,1),
                Amount = 100,
                Frequency = Frequency.Weekly
            });

            var service = NewTransactionService();
            var updatedTransaction = new Transaction{
                 Id = 1,
                Name = "Transaction Test",
                StartDate = new DateTime(2025,1,1),
                Amount = 200,
                Frequency = Frequency.Weekly
            };
            var result = service.Update(updatedTransaction);

            _transRepoMock.Verify(x => x.Update(It.IsAny<Transaction>()), Times.Once());
        }
    }
}
