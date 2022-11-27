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
using Moneyman.Tests.Extensions;
using Snapper;
using Snapper.Core;
using AutoMapper;
using Moneyman.Domain.MapperProfiles;

namespace Moneyman.Tests
{
    [TestClass]
    public class TransactionDtoTests
    {       
        private MapperConfiguration mapperConfig;

        [TestInitialize]
        public void SetUp()
        {
            mapperConfig = new MapperConfiguration(cfg => {
                cfg.AddProfile<TransactionDtoToTransactionProfile>();
            });
            
        }

        [TestMethod]
        public void AssertConfigurationIsValid_ReturnsSuccess()
        {
            mapperConfig.AssertConfigurationIsValid();
        }
    }
}
