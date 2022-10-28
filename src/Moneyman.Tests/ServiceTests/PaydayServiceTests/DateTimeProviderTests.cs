using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moneyman.Interfaces;
using Moneyman.Services;
using AutoMapper;
using System.Linq;
using FluentAssertions;
using Snapper;
using System.Collections.Generic;
using Moneyman.Domain;
using System;
using FluentAssertions.Extensions;

namespace Moneyman.Tests
{
    [TestClass]
    public class DateTimeProviderTests
    {
        private Mock<IDateTimeProvider> mockDateTimeProvider;
        private DateTimeProvider NewDateTimeProvider() 
            => new DateTimeProvider();

        [TestInitialize]
        public void SetUp()
        {            
            mockDateTimeProvider = new Mock<IDateTimeProvider>();
        }

        [TestMethod]
        public void GetToday_WithDate_ReturnsDate()
        {
            var paydayService = NewDateTimeProvider();
            var result = paydayService.GetToday();

            result.Should().Be(DateTime.Today);
        }

        [TestMethod]
        public void GetNow_WithDate_ReturnsDate()
        {
            var paydayService = NewDateTimeProvider();
            var result = paydayService.GetNow();

            result.Should().BeCloseTo(DateTime.Now, 5.Seconds());
        }
    }
}
