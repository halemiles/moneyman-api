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
using AutoFixture;
using AutoMapper;
using Moneyman.Domain.MapperProfiles;
using Microsoft.Extensions.Logging;

namespace Moneyman.Tests
{
    [TestClass]
    public class DtpReaderTests
    {
        private Mock<IPlanDateRepository> mockPlanDateRepository;
        private Mock<IOffsetCalculationService> mockOffsetCalculationService;
        private Mock<IPaydayService> mockPaydayService;
        private Mock<IDateTimeProvider> mockDateTimeProvider;
        private IMapper mockMapper;
        private Mock<ILogger<DtpReaderService>> mockLogger;

        private readonly List<string> holidays = new List<string> 
        {
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

        private DtpReaderService NewDtpReaderService() =>
            new DtpReaderService(
                    mockPlanDateRepository.Object,
                    mockPaydayService.Object,
                    mockDateTimeProvider.Object,
                    mockMapper,
                    mockLogger.Object
            );

        [TestInitialize]
        public void SetUp()
        {
            mockPlanDateRepository = new Mock<IPlanDateRepository>();
            mockOffsetCalculationService = new Mock<IOffsetCalculationService>();
            mockPaydayService = new Mock<IPaydayService>();
            mockDateTimeProvider = new Mock<IDateTimeProvider>();
            mockLogger = new Mock<ILogger<DtpReaderService>>();

            mockOffsetCalculationService.Setup(x => x.CalculateOffset(It.IsAny<DateTime>()))
                .Returns(new CalculatedPlanDate());

            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new PlanDateDtoProfile());
            });
            
            IMapper mapper = mappingConfig.CreateMapper();
            mockMapper = mapper;
        }    

        [TestMethod]
        public void GenerateMonthly_WithInvalidTransactionId_ReturnsEmptyList()
        {
            // Arrange
            var sut = NewDtpReaderService();
            Fixture fixture = new Fixture();
            mockPlanDateRepository.Setup(x => x.GetAll()).Returns(new List<PlanDate>()
            {
                new PlanDate
                {
                    Date = new DateTime(2022,11,1),
                    Transaction = new Transaction{
                        Amount = 100
                    } 
                },
                new PlanDate
                {
                    Date = new DateTime(2022,11,1),
                    Transaction = new Transaction{
                        Amount = 100,
                        
                    } 
                }
            });
            
            mockDateTimeProvider.Setup(x => x.GetToday()).Returns(new DateTime(2022,1,1));
            mockPaydayService.Setup(x => x.GetNext()).Returns(new Payday{Date = new DateTime(2022,12,1)});

            // Act
            var result = sut.GetCurrent();

            // Assert
            result.PlanDates.Count().Should().Be(2);
            result.AmountDue.Should().Be(200);
            result.ShouldMatchSnapshot();
        }
    }
}
