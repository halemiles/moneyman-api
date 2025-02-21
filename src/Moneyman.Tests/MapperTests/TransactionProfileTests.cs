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
    public class TransactionProfileTests
    {
        private MapperConfiguration mapperConfig;
        private IMapper mapper;

        [TestInitialize]
        public void SetUp()
        {
            mapperConfig = new MapperConfiguration(cfg => {
                cfg.AddProfile<TransactionProfile>();
            });

            mapper = mapperConfig.CreateMapper();

        }

        [TestMethod]
        public void AssertConfigurationIsValid_ReturnsSuccess()
        {
            mapperConfig.AssertConfigurationIsValid();
        }

        [TestMethod]
        public void Map_WithNullValues_ReturnsSuccess()
        {
            var existing = new Transaction
            {
                Id = 999,
                Name = "Test Transaction",
                Amount = 1234,
                StartDate = new DateTime(2022, 1, 1),
                Active = true,
                Frequency = Frequency.Monthly,
                IsAnticipated = false
            };

            var updated = new Transaction
            {
                Id = 999
            };

            var result = mapper.Map(updated, existing);
            result.Id.Should().Be(existing.Id);

            result.Name.Should().Be(existing.Name);
            result.Name.Should().NotBeNull();

            result.Amount.Should().Be(existing.Amount);
            result.StartDate.Should().Be(existing.StartDate);
            result.Active.Should().Be(existing.Active);
            result.Frequency.Should().Be(existing.Frequency);
            result.IsAnticipated.Should().Be(existing.IsAnticipated);
        }
    }
}
