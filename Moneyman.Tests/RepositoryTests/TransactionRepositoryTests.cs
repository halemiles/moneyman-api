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
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Threading;
using AutoMapper;
using Moneyman.Domain.MapperProfiles;

namespace Tests
{
    [TestClass]
    public class TransactionRepositoryTests
    {
        private Mock<DbSet<Transaction>> _dbSetMock;
        private Mock<MoneymanContext> _contextMock;   
        private Mock<TransactionRepository> _transRepoMock;    
        private Mock<IRepository<Transaction>> _genericRepositoryMock; 
        private Mock<DbSet<Transaction>> _transactions;
        private IMapper _mapper;
        private TransactionRepository NewTransactionRepository() =>
            new TransactionRepository(_contextMock.Object, _mapper);
        
        [TestInitialize]
        public void SetUp()
        {
            _dbSetMock = new  Mock<DbSet<Transaction>>();
            _contextMock = new  Mock<MoneymanContext>();     
            _transRepoMock = new Mock<TransactionRepository>(); //(_contextMock.Object);    
            _genericRepositoryMock = new Mock<IRepository<Transaction>>();   
            
            _transactions = new List<Transaction>()
            {
                new Transaction(){Name = "Transaction 1"},
                new Transaction(){Name = "Transaction 2"}
            }.AsQueryable().BuildMockDbSet();
            _contextMock.Setup(x => x.Set<Transaction>()).Returns(_transactions.Object);
            
            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new TransactionDtoToTransactionProfile());
                mc.AddProfile(new TransactionToTransactionDtoProfile());
                mc.AddProfile(new TransactionProfile());
            });
            IMapper mapper = mappingConfig.CreateMapper();
            _mapper = mapper;
        }

        [TestMethod]
        public void GetAll_WhenNoResults_ReturnsEmptyList()
        {
            _contextMock.Setup(x => x.Set<Transaction>()).Returns(new List<Transaction>(){}.AsQueryable().BuildMockDbSet().Object);
            var repository = NewTransactionRepository();
            var result = repository.GetAll();               
            result.Count().Should().Be(0);
        }

        [TestMethod]
        public void GetAll_WhenOneResult_ReturnsOneResult()
        {
            var repository = NewTransactionRepository();
            var result = repository.GetAll();               
            result.Count().Should().Be(2);
        }

        [TestMethod]
        public void Add_WithOneNewTransaction_SaveReturnsOneRecordCount()
        {
            var newTransaction = new TransactionBuilder()
                .WithId(1)
                .WithAmount(100)
                .WithActive(true)
                .WithFrequency(Frequency.Monthly)
                .WithStartDate(new DateTime(2021,1,1))
                .Build();
                
            Transaction existingTransaction = null;
            using (var context = new MoneymanContext(BuildGenerateInMemoryOptions()))
            {
                context.Transactions.Add(newTransaction);
                context.SaveChanges();
                existingTransaction = context.Transactions.FirstOrDefault();
            }

            existingTransaction.Should().NotBeNull();
            existingTransaction.Id.Should().Be(1);
            existingTransaction.Amount.Should().Be(100);
            existingTransaction.Active.Should().Be(true);
            existingTransaction.Frequency.Should().Be(Frequency.Monthly);
            existingTransaction.Date.Should().Be(new DateTime(2021,1,1));
        }

        [TestMethod] 
        public async Task Update_WithNewValidParams_PropertiesUpdated()
        {
            //var transactionRepository = NewTransactionRepository();
            var existingTransaction = new TransactionBuilder()
                .WithId(1)
                .WithAmount(100)
                .WithActive(true)
                .WithFrequency(Frequency.Monthly)
                .WithStartDate(new DateTime(2021,1,1))
                .Build();

            var transactionUpdate = new TransactionBuilder()
                .WithId(1)
                .WithAmount(500)
                .WithActive(false)
                .WithFrequency(Frequency.Weekly)
                .WithStartDate(new DateTime(2021,10,1))
                .Build();
                
            Transaction updatedTransaction = null;
            using (var context = new MoneymanContext(BuildGenerateInMemoryOptions()))
            {
                var transactionRepository = new TransactionRepository(context, _mapper);
                transactionRepository.Add(existingTransaction);
                await transactionRepository.Save();
                
                transactionRepository.Update(transactionUpdate);
                await transactionRepository.Save();
                
                updatedTransaction = context.Transactions.FirstOrDefault();
            }

            updatedTransaction.Should().NotBeNull();
            updatedTransaction.Id.Should().Be(1);
            updatedTransaction.Amount.Should().Be(500);
            updatedTransaction.Active.Should().Be(false);
            updatedTransaction.Frequency.Should().Be(Frequency.Weekly);
            updatedTransaction.Date.Should().Be(new DateTime(2021,10,1));
        }

        public List<Transaction> GenerateTrans()
        {
            List<Transaction> trans = new List<Transaction>();

            for(int i=0; i< 10; i++)
            {
                trans.Add(new Transaction(){
                    Name = $"Item {i}"
                });
            }

            return trans;
        }

        public DbContextOptions<MoneymanContext> BuildGenerateInMemoryOptions()
        {
            return new DbContextOptionsBuilder<MoneymanContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }
    }
}
