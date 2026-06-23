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

namespace Moneyman.Tests
{
    [TestClass]
    public class planDateServiceTests
    {
        private Mock<IPlanDateRepository> _planDateRepoMock;
        private Mock<ITransactionRepository> _transRepoMock;
        private Mock<ILogger<TransactionService>> mockTransactionServiceLogger;
        private Mock<TransactionMapper> transactionMapper;
        private PlanDateService NewPlanDateService() =>
            new PlanDateService(
                _planDateRepoMock.Object
            );

        private TransactionService NewTransactionService() =>
            new TransactionService(
                _transRepoMock.Object,
                 mockTransactionServiceLogger.Object,
                transactionMapper.Object,
                new Moneyman.Services.Validators.TransactionDtoValidator());

        [TestInitialize]
        public void SetUp()
        {
            _planDateRepoMock = new Mock<IPlanDateRepository>();
            _transRepoMock = new Mock<ITransactionRepository>();
            mockTransactionServiceLogger = new Mock<ILogger<TransactionService>>();
            transactionMapper = new Mock<TransactionMapper>();

        }

        [TestMethod]
        public void GetAll_WhenNoResults_ReturnsEmptyList()
        {
            _planDateRepoMock.Setup(x => x.GetAll()).Returns(new List<PlanDate>());

            var service = NewPlanDateService();
            var result = service.GetAll();
            result.Count.Should().Be(0);
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
            result.Count.Should().Be(5);
        }

        [TestMethod]
        public void MarkAsPaid_WhenPlanDateExists_ReturnsSuccess()
        {
            var planDate = new PlanDate { Id = 1, Paid = false };
            _planDateRepoMock.Setup(x => x.Get(1)).Returns(planDate);
            _planDateRepoMock.Setup(x => x.Update(It.IsAny<PlanDate>())).Returns(true);

            var service = NewPlanDateService();
            var result = service.MarkAsPaid(1);

            result.Success.Should().BeTrue();
            result.Payload.Paid.Should().BeTrue();
            _planDateRepoMock.Verify(x => x.Update(It.Is<PlanDate>(p => p.Paid == true)), Times.Once());
        }

        [TestMethod]
        public void MarkAsPaid_WhenPlanDateDoesNotExist_ReturnsNotFound()
        {
            _planDateRepoMock.Setup(x => x.Get(99)).Returns((PlanDate)null);

            var service = NewPlanDateService();
            var result = service.MarkAsPaid(99);

            result.Success.Should().BeFalse();
            result.StatusCode.Should().Be(StatusCode.NotFound);
            _planDateRepoMock.Verify(x => x.Update(It.IsAny<PlanDate>()), Times.Never());
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
            var result = await service.Create(newTransaction);

            _transRepoMock.Verify(x => x.Add(It.IsAny<Transaction>()), Times.Once());
            _transRepoMock.Verify(x => x.Save(), Times.Once());
            result.Payload.Should().Be(0);
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
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
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
