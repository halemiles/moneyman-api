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

namespace Tests
{
    [TestClass]
    public class PaydayRepositoryTests
    {
        private Mock<MoneymanContext> _contextMock;
        private IMapper _mapper;
        private List<Payday> _paydays;
        private PaydayRepository NewPaydayRepository() =>
            new PaydayRepository(_contextMock.Object, _mapper);
        
        [TestInitialize]
        public void SetUp()
        {
            _contextMock = new  Mock<MoneymanContext>(); 
            
            _paydays = new List<Payday>
            {
                new Payday {Date = new DateTime(2022,1,1)},
                new Payday {Date = new DateTime(2022,2,1)}
            };
            
            var paydayMockDbSet = _paydays.AsQueryable().BuildMockDbSet();

            _contextMock.Setup(x => x.Set<Payday>()).Returns(paydayMockDbSet.Object); 

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
            _contextMock.Setup(x => x.Set<Payday>()).Returns(new List<Payday>{}.AsQueryable().BuildMockDbSet().Object);
            var repository = NewPaydayRepository();
            var result = repository.GetAll();               
            result.Count().Should().Be(0);
        }

        [TestMethod]
        public void GetAll_WhenOneResult_ReturnsOneResult()
        {
            var repository = NewPaydayRepository();
            var result = repository.GetAll();               
            result.Count().Should().Be(2);
        }

        [TestMethod] 
        public async Task Add_WithMultiplePaydays_ReturnsSuccess()
        {                
            List<Payday> updatedPaydays = null;
            using (var context = new MoneymanContext(BuildGenerateInMemoryOptions()))
            {
                var paydayRepository = new PaydayRepository(context, _mapper);
                foreach(var payday in _paydays)
                {
                    paydayRepository.Add(payday);
                }
                await paydayRepository.Save();
                
                updatedPaydays = context.Paydays.ToList();
            }

            updatedPaydays.Should().NotBeNull();
            updatedPaydays.Count.Should().Be(_paydays.Count);
            
            updatedPaydays.ShouldMatchSnapshot();
        }

        [TestMethod] 
        [Ignore("Needs additonal setup")]
        public async Task RemoveAll_WithMultiplePaydays_ReturnsSuccess()
        {                
            List<Payday> updatedPaydays = null;
            using (var context = new MoneymanContext(BuildGenerateInMemoryOptions()))
            {
                var paydayRepository = new PaydayRepository(context, _mapper);
                foreach(var payday in _paydays)
                {
                    paydayRepository.Add(payday);
                }
                await paydayRepository.Save();
                paydayRepository.RemoveAll("Payday");
                updatedPaydays = context.Paydays.ToList();
            }

            updatedPaydays.Should().NotBeNull();
            updatedPaydays.Count.Should().Be(0);
        }

        public DbContextOptions<MoneymanContext> BuildGenerateInMemoryOptions()
        {
            return new DbContextOptionsBuilder<MoneymanContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }
    }
}
