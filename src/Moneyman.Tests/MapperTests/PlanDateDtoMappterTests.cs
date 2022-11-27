using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moneyman.Domain;
using System;
using Snapper;
using AutoMapper;
using Moneyman.Domain.MapperProfiles;
using Moneyman.Models.Dtos;

namespace Moneyman.Tests
{
    [TestClass]
    public class PlanDateDtoMappterTests
    {       
        private MapperConfiguration mapperConfig;
        private IMapper mapper;

        [TestInitialize]
        public void SetUp()
        {
            mapperConfig = new MapperConfiguration(cfg => {
                cfg.AddProfile<PlanDateDtoProfile>();
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
            var dto = new PlanDateDto
            {
                TransactionName = "Test Transaction",
                Amount = 1234,
                Date = new DateTime(2022,1,1)
            };

            var result = mapper.Map<PlanDateDto, PlanDate>(dto);

            result.ShouldMatchSnapshot();
        }

        [TestMethod]
        public void Map_WithValidModel_ReturnsSuccess()
        {
            var planDate = new PlanDate
            {
                Transaction = new Transaction
                {
                    Name = "Test Transaction",
                    Amount = 1234,
                    StartDate = new DateTime(2022,1,1)
                }
            };

            var result = mapper.Map<PlanDate, PlanDateDto>(planDate);

            result.ShouldMatchSnapshot();
        }
    }
}
