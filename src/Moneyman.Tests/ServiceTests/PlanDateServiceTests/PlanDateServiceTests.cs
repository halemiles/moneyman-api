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
using AutoMapper;
using Moneyman.Domain.MapperProfiles;
using Snapper;
using Microsoft.Extensions.Logging;
using AutoFixture;

namespace Tests
{
    [TestClass]
    public class planDateServiceTests
    {
        private Mock<IPlanDateRepository> _planDateRepoMock;
        private Mock<ITransactionRepository> _transRepoMock;
        private Mock<ILogger<PlanDateService>> mockLogger;
        private Mock<ILogger<TransactionService>> mockTransactionServiceLogger;
        private PlanDateService NewPlanDateService() =>
            new PlanDateService(
                _planDateRepoMock.Object,
                mockLogger.Object
            );

        private TransactionService NewTransactionService() =>
            new TransactionService(_transRepoMock.Object, mockTransactionServiceLogger.Object);
        
        [TestInitialize]
        public void SetUp()
        {
            _planDateRepoMock = new Mock<IPlanDateRepository>();
            mockLogger = new Mock<ILogger<PlanDateService>>();
            _transRepoMock = new Mock<ITransactionRepository>();
            mockTransactionServiceLogger = new Mock<ILogger<TransactionService>>();
        }

        [TestMethod]
        public void GetAll_WhenNoResults_ReturnsEmptyList()
        {
            _planDateRepoMock.Setup(x => x.GetAll()).Returns(new List<PlanDate>());
            
            var service = NewPlanDateService();
            var result = service.GetAll();
            result.Count().Should().Be(0);
        }

        [TestMethod]
        public void GetAll_WhenMultipleResults_ReturnsList()
        {
            Fixture fixture = new Fixture();
            List<PlanDate> planDateFixture = new List<PlanDate>
            {
                fixture.Create<PlanDate>(),
                fixture.Create<PlanDate>(),
                fixture.Create<PlanDate>(),
                fixture.Create<PlanDate>(),
                fixture.Create<PlanDate>()
            };
            _planDateRepoMock.Setup(x => x.GetAll()).Returns(planDateFixture);
            
            var service = NewPlanDateService();
            var result = service.GetAll();
            result.Count().Should().Be(5);
        }

        // [TestMethod]
        // public void Search_WhenNoResults_ReturnsEmptyList()
        // {
        //     _planDateRepoMock.Setup(x => x.Search(It.IsAny<string>())).Returns(new List<PlanDate>());
            
        //     var service = NewPlanDateService();
        //     var result = service.Search(string.Empty);
        //     result.Count().Should().Be(0);
        // }

        [TestMethod]
        public void Search_WhenObjectDoesntExist_ReturnsSuccess()
        {
            var newTransaction = new Transaction
            {
                Name = "newTransaction",
                StartDate = new DateTime(2022,1,1),
                Amount = 150,
                Frequency = Frequency.Weekly
            };

            _transRepoMock.Setup(x => x.Update(It.IsAny<Transaction>()))
                .Returns(true);
            
            var service = NewTransactionService();
            var result = service.Update(newTransaction);               
            
            _transRepoMock.Verify(x => x.Update(It.IsAny<Transaction>()), Times.Once());
            _transRepoMock.Verify(x => x.Save(), Times.Once());
            result.Should().Be(0);
        }

        [TestMethod]
        public void Create_WhenObjectDoesntExist_ReturnsSuccess()
        {
            var newTransaction = new Transaction
            {
                Name = "newTransaction",
                StartDate = new DateTime(2022,1,1),
                Amount = 150,
                Frequency = Frequency.Weekly
            };

            
            var service = NewTransactionService();
            var result = service.Create(newTransaction);               
                        
            _transRepoMock.Verify(x => x.Add(It.IsAny<Transaction>()), Times.Once());
            _transRepoMock.Verify(x => x.Save(), Times.Once());
            result.Should().Be(true);
        }

        [TestMethod]
        [DataRow(null, 100, "2022-01-01")]
        [DataRow("", 100, "2022-01-01")]
        [DataRow("TransactionName", 0, "2022-01-01")]
        [DataRow("TransactionName", 100, "1/1/0001 12:00:00 AM")]
        public void Create_WhenObjectDoesntExist_ReturnsFailure(
            string transactionName,
            int amount,
            string startDate
        )
        {
            var newTransaction = new Transaction
            {
                Name = transactionName,
                StartDate = DateTime.Parse(startDate),
                Amount = amount,
                Frequency = Frequency.Weekly
            };

            
            var service = NewTransactionService();
            var result = service.Create(newTransaction);               
                        
            _transRepoMock.Verify(x => x.Add(It.IsAny<Transaction>()), Times.Never());
            _transRepoMock.Verify(x => x.Save(), Times.Never());
            result.Should().Be(false);
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
    }
}
