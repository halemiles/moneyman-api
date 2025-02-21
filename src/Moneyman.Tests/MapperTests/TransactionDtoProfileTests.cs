using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moneyman.Domain;
using System;
using Snapper;
using AutoMapper;
using Moneyman.Domain.MapperProfiles;
using FluentAssertions;

namespace Moneyman.Tests
{
    [TestClass]
    public class TransactionDtoProfileTests
    {
        private MapperConfiguration mapperConfig;
        private IMapper mapper;

        [TestInitialize]
        public void SetUp()
        {
            mapperConfig = new MapperConfiguration(cfg => {
                cfg.AddProfile<TransactionDtoProfile>();
            });

            mapper = mapperConfig.CreateMapper();

        }

        [TestMethod]
        public void AssertConfigurationIsValid_ReturnsSuccess()
        {
            mapperConfig.AssertConfigurationIsValid();
        }

        [TestMethod]
        public void Map_WithValidDto_ReturnsSuccess()
        {
            var dto = new TransactionDto
            {
                Id = 999,
                Name = "Test Transaction",
                Amount = 1234,
                StartDate = new DateTime(2022,1,1),
                Active = true,
                Frequency = Frequency.Monthly
            };

            var result = mapper.Map<TransactionDto, Transaction>(dto);

            result.ShouldMatchSnapshot();
        }

        [TestMethod]
        public void Map_WithValidModel_ReturnsSuccess()
        {
            var transaction = new Transaction
            {
                Id = 999,
                Name = "Test Transaction",
                Amount = 1234,
                StartDate = new DateTime(2022,1,1),
                Active = true,
                Frequency = Frequency.Monthly
            };

            var result = mapper.Map<Transaction, TransactionDto>(transaction);
            result.ShouldMatchSnapshot();
        }

        [TestMethod]
        public void Map_WhenDataUpdated_ReturnsSuccess()
        {
            var existing = new Transaction
            {
                Id = 999,
                Name = "Test Transaction",
                Amount = 1234,
                StartDate = new DateTime(2022,1,1),
                Active = true,
                Frequency = Frequency.Monthly
            };

            var updated = new TransactionDto
            {
                Id = 999,
                Name = "Updated Transaction",
                Amount = 4321,
                StartDate = new DateTime(2021,1,1),
                Active = true,
                Frequency = Frequency.Yearly
            };

            var result = mapper.Map(updated,existing);
            result.Id.Should().Be(updated.Id);
            result.Name.Should().BeSameAs(updated.Name);
            result.Amount.Should().Be(updated.Amount);
            result.StartDate.Should().Be(updated.StartDate);
            //result.Active.Should().Be(updated.Active);
            result.Frequency.Should().Be(updated.Frequency);
        }
    }
}
