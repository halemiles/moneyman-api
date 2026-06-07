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
        public void GetCurrent_WithBankAccountId_ReturnsOnlyPlanDatesForThatBankAccount()
        {
            // Arrange
            var sut = NewDtpService();
            mockPlanDateRepository.Setup(x => x.GetAll()).Returns(new List<PlanDate>
            {
                PlanDateFor(bankAccountId: 1),
                PlanDateFor(bankAccountId: 1),
                PlanDateFor(bankAccountId: 2)
            });
            mockDateTimeProvider.Setup(x => x.GetToday()).Returns(new DateTime(2022,1,1));
            mockPaydayService.Setup(x => x.GetNext()).Returns(new Payday{Date = new DateTime(2022,12,1)});

            // Act
            var result = sut.GetCurrent(null, bankAccountId: 1);

            // Assert
            result.Payload.PlanDates.Should().HaveCount(2);
            result.Payload.PlanDates.Should().OnlyContain(pd => pd.BankAccountId == 1);
        }

        [TestMethod]
        public void GetCurrent_WithoutBankAccountId_ReturnsPlanDatesForAllBankAccounts()
        {
            // Arrange
            var sut = NewDtpService();
            mockPlanDateRepository.Setup(x => x.GetAll()).Returns(new List<PlanDate>
            {
                PlanDateFor(bankAccountId: 1),
                PlanDateFor(bankAccountId: 2),
                PlanDateFor(bankAccountId: 3)
            });
            mockDateTimeProvider.Setup(x => x.GetToday()).Returns(new DateTime(2022,1,1));
            mockPaydayService.Setup(x => x.GetNext()).Returns(new Payday{Date = new DateTime(2022,12,1)});

            // Act
            var result = sut.GetCurrent(null);

            // Assert
            result.Payload.PlanDates.Should().HaveCount(3);
        }

        [TestMethod]
        public void GetCurrent_WithUnknownBankAccountId_ReturnsNoPlanDates()
        {
            // Arrange
            var sut = NewDtpService();
            mockPlanDateRepository.Setup(x => x.GetAll()).Returns(new List<PlanDate>
            {
                PlanDateFor(bankAccountId: 1),
                PlanDateFor(bankAccountId: 2)
            });
            mockDateTimeProvider.Setup(x => x.GetToday()).Returns(new DateTime(2022,1,1));
            mockPaydayService.Setup(x => x.GetNext()).Returns(new Payday{Date = new DateTime(2022,12,1)});

            // Act
            var result = sut.GetCurrent(null, bankAccountId: 99);

            // Assert
            result.Payload.PlanDates.Should().BeEmpty();
        }

        [TestMethod]
        public void GetOffset_WithBankAccountId_ReturnsOnlyPlanDatesForThatBankAccount()
        {
            // Arrange
            var sut = NewDtpService();
            mockPlanDateRepository.Setup(x => x.GetAll()).Returns(new List<PlanDate>
            {
                PlanDateFor(bankAccountId: 1),
                PlanDateFor(bankAccountId: 2)
            });
            mockPaydayService.Setup(x => x.GetPrevious()).Returns(new Payday{Date = new DateTime(2022,1,1)});
            mockPaydayService.Setup(x => x.GetNext()).Returns(new Payday{Date = new DateTime(2022,12,1)});

            // Act
            var result = sut.GetOffset(0, bankAccountId: 2);

            // Assert
            result.Payload.PlanDates.Should().HaveCount(1);
            result.Payload.PlanDates.Should().OnlyContain(pd => pd.BankAccountId == 2);
        }

        private static PlanDate PlanDateFor(int bankAccountId) =>
            new PlanDate
            {
                Date = new DateTime(2022,11,1),
                Transaction = new Transaction
                {
                    Amount = 100,
                    Frequency = Frequency.Monthly,
                    BankAccountId = bankAccountId,
                    Active = true
                }
            };
    }
}
