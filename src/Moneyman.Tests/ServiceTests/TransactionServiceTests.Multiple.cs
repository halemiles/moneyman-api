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
using AutoFixture;

namespace Tests
{
    [TestClass]
    public class TransactionServiceTestsMultiple
    {
        private Mock<ITransactionRepository> _transRepoMock;
        private TransactionService NewTransactionService() =>
            new TransactionService(_transRepoMock.Object);
        
        [TestInitialize]
        public void SetUp()
        {
            _transRepoMock = new Mock<ITransactionRepository>();
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
