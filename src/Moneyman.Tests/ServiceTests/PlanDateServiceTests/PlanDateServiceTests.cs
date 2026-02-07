using Moneyman.Interfaces;
using Moneyman.Services;
using Moq;
using Moneyman.Domain;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using AutoFixture;

namespace Moneyman.Tests
{
    [TestClass]
    public class planDateServiceTests
    {
        private Mock<IPlanDateRepository> _planDateRepoMock;
        private Mock<ILogger<PlanDateService>> mockLogger;
        private PlanDateService NewPlanDateService() =>
            new PlanDateService(
                _planDateRepoMock.Object,
                mockLogger.Object
            );

        [TestInitialize]
        public void SetUp()
        {
            _planDateRepoMock = new Mock<IPlanDateRepository>();
            mockLogger = new Mock<ILogger<PlanDateService>>();

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
        // public void Search_WhenObjectDoesntExist_ReturnsSuccess()
        // {
        //     var newTransaction = new Transaction
        //     {
        //         Name = "newTransaction",
        //         StartDate = new DateTime(2022,1,1),
        //         Amount = 150,
        //         Frequency = Frequency.Weekly
        //     };

        //     _transRepoMock.Setup(x => x.Update(It.IsAny<Transaction>()))
        //         .Returns(true);

        //     var service = NewTransactionService();
        //     var result = service.Update(newTransaction);

        //     _transRepoMock.Verify(x => x.Update(It.IsAny<Transaction>()), Times.Once());
        //     _transRepoMock.Verify(x => x.Save(), Times.Once());
        //     result.Should().Be(0);
        // }

    }
}
