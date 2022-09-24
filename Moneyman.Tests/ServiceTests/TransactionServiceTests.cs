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

namespace Tests
{
    [TestClass]
    public class TransactionServiceTests
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
    }
}
