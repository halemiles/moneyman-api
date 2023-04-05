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
using AutoFixture;

namespace Tests
{
    [TestClass]
    public class PlanDateRepositoryTests
    {
        private Mock<MoneymanContext> _contextMock;
        private IMapper _mapper;
        private List<PlanDate> _planDates;
        private PlanDateRepository NewPlanDateRepository() =>
            new PlanDateRepository(_contextMock.Object, _mapper);
        
        [TestInitialize]
        public void SetUp()
        {
            _contextMock = new  Mock<MoneymanContext>(); 
            var fixture = new Fixture();

            var transaction1 = new Transaction
            {
                Name = "Example transaction",
                Amount = 123.45m,
                Active = true,
                StartDate = new DateTime(2022, 03, 01),
                Frequency = Frequency.Monthly,
                IsAnticipated = false,
                PaymentType = PaymentType.DIRECTDEBIT,
                CategoryType = CategoryType.ENTERTAINMENT,
                PriorityType = PriorityType.DEPENDANT
            };

            var planDate1 = new PlanDate
            {
                Transaction = transaction1,
                Date = new DateTime(2022, 03, 01),
                Active = true,
                OriginalDate = new DateTime(2022, 03, 01),
                YearGroup = 2022,
                MonthGroup = 3,
                IsAnticipated = false,
                OrderId = 1
            };

            var transaction2 = new Transaction
            {
                Name = "Example transaction",
                Amount = 123.45m,
                Active = true,
                StartDate = new DateTime(2022, 03, 01),
                Frequency = Frequency.Monthly,
                IsAnticipated = false,
                PaymentType = PaymentType.DIRECTDEBIT,
                CategoryType = CategoryType.ENTERTAINMENT,
                PriorityType = PriorityType.DEPENDANT
            };

            var planDate2 = new PlanDate
            {
                Transaction = transaction2,
                Date = new DateTime(2022, 03, 01),
                Active = true,
                OriginalDate = new DateTime(2022, 03, 01),
                YearGroup = 2022,
                MonthGroup = 3,
                IsAnticipated = false,
                OrderId = 1
            };

            _planDates = new List<PlanDate>
            {
                planDate1,
                planDate2
            };
            
            var planDateMockDbSet = _planDates.AsQueryable().BuildMockDbSet();

            _contextMock.Setup(x => x.Set<PlanDate>()).Returns(planDateMockDbSet.Object); 

            var mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new TransactionDtoToTransactionProfile());
                mc.AddProfile(new TransactionProfile());
            });
            
            IMapper mapper = mappingConfig.CreateMapper();
            _mapper = mapper;           
        }

        [TestMethod]
        public void GetAll_WhenNoResults_ReturnsEmptyList()
        {
            List<PlanDate> updatedPlanDates = null;
            
            using (var context = new MoneymanContext(BuildGenerateInMemoryOptions()))
            {
                var planDateRepository = new PlanDateRepository(context, _mapper);
                updatedPlanDates = planDateRepository.GetAll().ToList(); 
            }
                          
            updatedPlanDates.Count().Should().Be(0);
        }

        [TestMethod]        
        public void GetAll_WhenOneResult_ReturnsOneResult()
        {
            List<PlanDate> updatedPlanDates = null;
            var fixture = new Fixture();
            using (var context = new MoneymanContext(BuildGenerateInMemoryOptions()))
            {
                var planDateRepository = new PlanDateRepository(context, _mapper);
                planDateRepository.Add(fixture.Create<PlanDate>());
                planDateRepository.Add(fixture.Create<PlanDate>());
                planDateRepository.Save();
                updatedPlanDates = planDateRepository.GetAll().ToList(); 
                
            }         
            updatedPlanDates.Count().Should().Be(2);
        }

        [TestMethod]
        public async Task Add_WithMultiplePaydays_ReturnsSuccess()
        {                
            List<PlanDate> updatedPlanDates = null;
            using (var context = new MoneymanContext(BuildGenerateInMemoryOptions()))
            {
                var planDateRepository = new PlanDateRepository(context, _mapper);
                foreach(var planDate in _planDates)
                {
                    planDateRepository.Add(planDate);
                }
                await planDateRepository.Save();
                
                updatedPlanDates = context.PlanDates.ToList();
            }

            updatedPlanDates.Should().NotBeNull();
            updatedPlanDates.Count.Should().Be(_planDates.Count);
            
            updatedPlanDates.ShouldMatchSnapshot();
        }

        public DbContextOptions<MoneymanContext> BuildGenerateInMemoryOptions()
        {
            return new DbContextOptionsBuilder<MoneymanContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }
    }
}
