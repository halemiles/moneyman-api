using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moneyman.Domain;
using System;
using Snapper;
using AutoMapper;
using Moneyman.Domain.MapperProfiles;

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
                cfg.AddProfile<TransactionDtoToTransactionProfile>();
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
    }
}
