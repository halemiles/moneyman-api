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

namespace Moneyman.Tests
{
    [TestClass]
    public class TransactionRepositoryTests
    {
        private Mock<DbSet<Transaction>> _dbSetMock;
        private Mock<MoneymanContext> _contextMock;
        private Mock<TransactionRepository> _transRepoMock;
        private Mock<IRepository<Transaction>> _genericRepositoryMock;
        private TransactionMapper _mapper;
        private TransactionRepository NewTransactionRepository() =>
            new TransactionRepository(_contextMock.Object);

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

            _mapper = new TransactionMapper();
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
            existingTransaction.StartDate.Should().Be(new DateTime(2021,1,1));
        }

        [TestMethod]
        public async Task Update_WithNewValidParams_PropertiesUpdated()
        {
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

            // Use separate contexts over the same in-memory store so the update
            // does not collide with the entity tracked by the insert (this mirrors
            // the per-request scoped DbContext the API uses in production).
            var options = BuildGenerateInMemoryOptions();

            using (var context = new MoneymanContext(options))
            {
                var transactionRepository = new TransactionRepository(context);
                transactionRepository.Add(existingTransaction);
                await transactionRepository.Save();
            }

            using (var context = new MoneymanContext(options))
            {
                var transactionRepository = new TransactionRepository(context);
                transactionRepository.Update(transactionUpdate);
                await transactionRepository.Save();
            }

            Transaction updatedTransaction;
            using (var context = new MoneymanContext(options))
            {
                updatedTransaction = context.Transactions.FirstOrDefault();
            }

            updatedTransaction.Should().NotBeNull();
            updatedTransaction.Id.Should().Be(1);
            updatedTransaction.Amount.Should().Be(500);
            updatedTransaction.Active.Should().BeFalse();
            updatedTransaction.Frequency.Should().Be(Frequency.Weekly);
            updatedTransaction.StartDate.Should().Be(new DateTime(2021,10,1));
        }

        public DbContextOptions<MoneymanContext> BuildGenerateInMemoryOptions()
        {
            return new DbContextOptionsBuilder<MoneymanContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }
    }
}
