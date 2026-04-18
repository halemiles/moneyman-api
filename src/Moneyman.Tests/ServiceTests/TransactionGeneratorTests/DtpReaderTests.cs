using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moneyman.Interfaces;
using Moneyman.Services;
using Moneyman.Domain;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using System;
using Snapper;
using Moneyman.Services.Interfaces;
using Moneyman.Domain.MapperProfiles;
using Microsoft.Extensions.Logging;

namespace Moneyman.Tests
{
    [TestClass]
    public class DtpReaderTests
    {
        private Mock<ITransactionRepository> mockTransactionRepository;
        private Mock<IPlanDateRepository> mockPlanDateRepository;
        private Mock<IOffsetCalculationService> mockOffsetCalculationService;
        private Mock<IPaydayService> mockPaydayService;
        private Mock<IDateTimeProvider> mockDateTimeProvider;
        private PlanDateMapper planDateMapper;
        private Mock<ILogger<DtpService>> mockLogger;

        private DtpService NewDtpService() =>
            new DtpService(
                    mockTransactionRepository.Object,
                    mockPlanDateRepository.Object,
                    mockOffsetCalculationService.Object,
                    mockPaydayService.Object,
                    mockDateTimeProvider.Object,
                    mockLogger.Object,
                    planDateMapper
            );

        [TestInitialize]
        public void SetUp()
        {
            mockPlanDateRepository = new Mock<IPlanDateRepository>();
            mockTransactionRepository = new Mock<ITransactionRepository>();
            mockOffsetCalculationService = new Mock<IOffsetCalculationService>();
            mockPaydayService = new Mock<IPaydayService>();
            mockDateTimeProvider = new Mock<IDateTimeProvider>();
            mockLogger = new Mock<ILogger<DtpService>>();

            mockOffsetCalculationService.Setup(x => x.CalculateOffset(It.IsAny<DateTime>()))
                .Returns(new CalculatedPlanDate());

            planDateMapper = new PlanDateMapper();
        }

        [TestMethod]
        public void GenerateMonthly_WithInvalidTransactionId_ReturnsEmptyList()
        {
            // Arrange
            var sut = NewDtpService();
            _ = mockPlanDateRepository.Setup(x => x.GetAll()).Returns(new List<PlanDate>
            {
                new PlanDate
                {
                    Date = new DateTime(2022,11,1),
                    Transaction = new Transaction{
                        Amount = 100,
                        Frequency = Frequency.Monthly,
                        BankAccountId = 0,
                        Active = true
                    }
                },
                new PlanDate
                {
                    Date = new DateTime(2022,11,1),
                    Transaction = new Transaction{
                        Amount = 100,
                        Frequency = Frequency.Monthly,
                        BankAccountId = 0,
                        Active = true

                    }
                }
            });

            mockDateTimeProvider.Setup(x => x.GetToday()).Returns(new DateTime(2022,1,1));
            mockPaydayService.Setup(x => x.GetNext()).Returns(new Payday{Date = new DateTime(2022,12,1)});

            // Act
            var result = sut.GetCurrent(null);

            // Assert
            result.Payload.PlanDates.Count().Should().Be(2);
            result.Payload.AmountDue.Should().Be(200);
            result.ShouldMatchSnapshot();
        }

        [TestMethod]
        public void GetCurrent_WhenPlanDateIsPaid_ExcludesFromResults()
        {
            // Arrange
            var sut = NewDtpService();
            _ = mockPlanDateRepository.Setup(x => x.GetAll()).Returns(new List<PlanDate>
            {
                new PlanDate
                {
                    Date = new DateTime(2022,11,1),
                    Paid = false,
                    Transaction = new Transaction{
                        Amount = 100,
                        Frequency = Frequency.Monthly,
                        BankAccountId = 0,
                        Active = true
                    }
                },
                new PlanDate
                {
                    Date = new DateTime(2022,11,1),
                    Paid = true,
                    Transaction = new Transaction{
                        Amount = 100,
                        Frequency = Frequency.Monthly,
                        BankAccountId = 0,
                        Active = true
                    }
                }
            });

            mockDateTimeProvider.Setup(x => x.GetToday()).Returns(new DateTime(2022,1,1));
            mockPaydayService.Setup(x => x.GetNext()).Returns(new Payday{Date = new DateTime(2022,12,1)});

            // Act
            var result = sut.GetCurrent(null);

            // Assert
            result.Payload.PlanDates.Count().Should().Be(1);
            result.Payload.AmountDue.Should().Be(100);
        }

        [TestMethod]
        public void GetCurrent_WhenAllPlanDatesArePaid_ReturnsEmptyList()
        {
            // Arrange
            var sut = NewDtpService();
            _ = mockPlanDateRepository.Setup(x => x.GetAll()).Returns(new List<PlanDate>
            {
                new PlanDate
                {
                    Date = new DateTime(2022,11,1),
                    Paid = true,
                    Transaction = new Transaction{
                        Amount = 100,
                        Frequency = Frequency.Monthly,
                        BankAccountId = 0,
                        Active = true
                    }
                },
                new PlanDate
                {
                    Date = new DateTime(2022,11,1),
                    Paid = true,
                    Transaction = new Transaction{
                        Amount = 200,
                        Frequency = Frequency.Monthly,
                        BankAccountId = 0,
                        Active = true
                    }
                }
            });

            mockDateTimeProvider.Setup(x => x.GetToday()).Returns(new DateTime(2022,1,1));
            mockPaydayService.Setup(x => x.GetNext()).Returns(new Payday{Date = new DateTime(2022,12,1)});

            // Act
            var result = sut.GetCurrent(null);

            // Assert
            result.Payload.PlanDates.Count().Should().Be(0);
            result.Payload.AmountDue.Should().Be(0);
        }
    }
}
