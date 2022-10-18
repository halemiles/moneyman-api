using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moneyman.Interfaces;
using Moneyman.Services;
using Moneyman.Domain;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using System;
using AutoFixture;
using AutoFixture.AutoMoq;

namespace Moneyman.Tests
{
    [TestClass]
    public class DtpMonthlyGenerationTests
    {
        private Mock<ITransactionService> mockTransactionService;
        private Mock<ITransactionRepository> mockTransactionRepository;
        private Mock<IPlanDateRepository> mockPlanDateRepository;
        private Mock<IOffsetCalculationService> mockOffsetCalculationService;

        private List<string> holidays = new List<string>(){
                "03-01-2022",
                "15-04-2022",
                "18-04-2022",
                "02-05-2022",
                "02-06-2022",
                "03-06-2022",
                "29-08-2022",
                "26-12-2022",
                "27-12-2022"
        };

        private DtpService NewDtpGenerationService() =>
            new DtpService(
                    mockTransactionRepository.Object,
                    mockPlanDateRepository.Object,
                    mockOffsetCalculationService.Object
            );

        [TestInitialize]
        public void SetUp()
        {
            mockTransactionService = new Mock<ITransactionService>();
            mockTransactionRepository = new Mock<ITransactionRepository>();
            mockPlanDateRepository = new Mock<IPlanDateRepository>();
            mockOffsetCalculationService = new Mock<IOffsetCalculationService>();

            mockOffsetCalculationService.Setup(x => x.CalculateOffset(It.IsAny<DateTime>()))
                .Returns(new DteObject());
        }

        [TestMethod]
        public void GenerateMonthly_WithInvalidTransactionId_ReturnsEmptyList()
        {
            // Arrange
            var sut = NewDtpGenerationService();
            IEnumerable<Transaction> trans = new List<Transaction>();
            mockTransactionRepository.Setup(x => x.GetAll()).Returns(trans);

            // Act
            var result = sut.GenerateMonthly(0);

            // Assert
            result.Count.Should().Be(0);
        }

        [TestMethod]
        public void GenerateMonthly_WithValidMonthlyTransaction_ReturnsSuccess()
        {
            // Arrange
            var sut = NewDtpGenerationService();
            IEnumerable<Transaction> trans = new List<Transaction>()
            {
                new Transaction(){
                    Name = "Trans 1",
                    Amount = 100,
                    Active = true,
                    StartDate = new DateTime(2022,1,1)
                }
            }.AsEnumerable();
            mockTransactionRepository.Setup(x => x.GetAll()).Returns(trans);

            // Act
            var result = sut.GenerateMonthly(0);

            // Assert
            result.Count.Should().Be(12);
            result.All(x => x.Transaction.Name == "Trans 1").Should().BeTrue();
        }

        [TestMethod]
        [DataRow("2022-01-08","2022-01-10","2022-02-08","2022-03-08","2022-04-08","2022-05-09","2022-06-08","2022-07-08","2022-08-08","2022-09-10","2022-10-08","2022-11-08","2022-12-08")]
        public void GenerateMonthly_WithExpectedValues_ReturnsSuccess(string startDateString,
            string mo1,string mo2,string mo3,string mo4,string mo5,string mo6,string mo7,string mo8,string mo9,string mo10,string mo11,string mo12
        )
        {
            // Arrange
            var startDate = DateTime.Parse(startDateString);
            var sut = NewDtpGenerationService();
            var fixture = new Fixture().Customize(new AutoMoqCustomization());

            IEnumerable<Transaction> trans = new List<Transaction>()
            {
                fixture.Build<Transaction>().With(f => f.StartDate, startDate).Create()
            }.AsEnumerable();
            mockTransactionRepository.Setup(x => x.GetAll()).Returns(trans);
            mockOffsetCalculationService.SetupSequence(x => x.CalculateOffset(It.IsAny<DateTime>()))
                .Returns(new DteObject{ PlanDate = DateTime.Parse(mo1)})
                .Returns(new DteObject{ PlanDate = DateTime.Parse(mo2)})
                .Returns(new DteObject{ PlanDate = DateTime.Parse(mo3)})
                .Returns(new DteObject{ PlanDate = DateTime.Parse(mo4)})
                .Returns(new DteObject{ PlanDate = DateTime.Parse(mo5)})
                .Returns(new DteObject{ PlanDate = DateTime.Parse(mo6)})
                .Returns(new DteObject{ PlanDate = DateTime.Parse(mo7)})
                .Returns(new DteObject{ PlanDate = DateTime.Parse(mo8)})
                .Returns(new DteObject{ PlanDate = DateTime.Parse(mo9)})
                .Returns(new DteObject{ PlanDate = DateTime.Parse(mo10)})
                .Returns(new DteObject{ PlanDate = DateTime.Parse(mo11)})
                .Returns(new DteObject{ PlanDate = DateTime.Parse(mo12)});
            // Act
            var result = sut.GenerateMonthly(0);

            // Assert
            result.Count.Should().Be(12);
            result[0].Date.Should().Be(DateTime.Parse(mo1));
            result[1].Date.Should().Be(DateTime.Parse(mo2));
            result[2].Date.Should().Be(DateTime.Parse(mo3));
            result[3].Date.Should().Be(DateTime.Parse(mo4));
            result[4].Date.Should().Be(DateTime.Parse(mo5));
            result[5].Date.Should().Be(DateTime.Parse(mo6));
            result[6].Date.Should().Be(DateTime.Parse(mo7));
            result[7].Date.Should().Be(DateTime.Parse(mo8));
            result[8].Date.Should().Be(DateTime.Parse(mo9));
            result[9].Date.Should().Be(DateTime.Parse(mo10));
            result[10].Date.Should().Be(DateTime.Parse(mo11));
            result[11].Date.Should().Be(DateTime.Parse(mo12));
        }
    }
}
