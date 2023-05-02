using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moneyman.Domain;
using Moneyman.Services.Extentions;
using Moq;

namespace Moneyman.Tests.Extensions
{
    [TestClass]
    public class FrequencyExtensionsTests
    {
        [DataTestMethod]
        [DataRow(Frequency.Daily, 365)]
        [DataRow(Frequency.Weekly, 52)]
        [DataRow(Frequency.Monthly, 12)]
        [DataRow(Frequency.Yearly, 1)]
        public void ToFrequencyCount_ReturnsCorrectCount(Frequency frequency, int expectedCount)
        {            
            // Act
            int actualCount = frequency.ToFrequencyCount();

            // Assert
            Assert.AreEqual(expectedCount, actualCount);
        }
    }
}