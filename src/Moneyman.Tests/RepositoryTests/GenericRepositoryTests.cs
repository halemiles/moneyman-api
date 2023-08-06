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
using Microsoft.Extensions.Configuration;
using System.Threading;

namespace Tests
{
    [TestClass]
    public class GenericRepositoryTests
    {
        private Mock<DbSet<Transaction>> _dbSetMock;
        private Mock<MoneymanContext> _contextMock;   
        private Mock<TransactionRepository> _transRepoMock;    
        private Mock<IRepository<Transaction>> _genericRepositoryMock;
        private IMapper _mapper;
        private GenericRepository<Transaction> repository;
        private TransactionRepository NewTransactionRepository() =>
            new TransactionRepository(_contextMock.Object, _mapper);
        
        [TestInitialize]
        public void SetUp()
        {
            _dbSetMock = new  Mock<DbSet<Transaction>>();
            _contextMock = new  Mock<MoneymanContext>();     
            _transRepoMock = new Mock<TransactionRepository>();
            _genericRepositoryMock = new Mock<IRepository<Transaction>>();   
            
            var _transactions = new List<Transaction>
            {
                new Transaction {Name = "Transaction 1"},
                new Transaction {Name = "Transaction 2"}
            }.AsQueryable().BuildMockDbSet();

            _contextMock.Setup(x => x.Set<Transaction>()).Returns(_transactions.Object);
            
            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new TransactionDtoToTransactionProfile());
                mc.AddProfile(new TransactionProfile());
            });
            
            IMapper mapper = mappingConfig.CreateMapper();
            _mapper = mapper;

            repository = new GenericRepository<Transaction>(_contextMock.Object, mapper);
        }

        [TestMethod]
        public void GetAll_WhenNoResults_ReturnsEmptyList()
        {
            _contextMock.Setup(x => x.Set<Transaction>()).Returns(new List<Transaction>{}.AsQueryable().BuildMockDbSet().Object);
            var repository = NewTransactionRepository();
            var result = repository.GetAll();               
            result.Count().Should().Be(0);
        }

        [TestMethod]
        public async Task Add_WithOneNewTransaction_SaveReturnsOneRecordCount()
        {  
            // Arrange
            var repository = new GenericRepository<Transaction>(_contextMock.Object, _mapper);
            var newTransaction = new Transaction();

            // Act
            repository.Add(newTransaction);
            await repository.Save();

            // Assert
            _contextMock.Verify(x => x.Set<Transaction>().Add(newTransaction), Times.Once);
            _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
