using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moneyman.Domain;
using FluentAssertions;
using System;

namespace Moneyman.Tests
{
    [TestClass]
    public class CalculatedPlanDateTests
    {

        [TestMethod]
        public void PlanDateString_WithDate_ReturnsStringInCorrectFormat()
        {
            // Arrange
            var sut = new CalculatedPlanDate
            {
                PlanDate = new DateTime(2022,10,20)
            };
            // Act
            var result = sut.PlanDateString;

            // Assert
            result.Should().Be("20-10-2022");
        }
    }
}
