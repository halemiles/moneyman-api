using Microsoft.Extensions.Logging;
using Moq;
using Moneyman.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Moneyman.Tests.Services
{
    [TestClass]
    public class BaseServiceTests
    {

        [TestMethod]
        public void Constructor_ShouldThrowArgumentNullException_WhenLoggerIsNull()
        {
            // Arrange
            ILogger<BaseService> logger = null;

            // Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => new BaseService(logger));
        }

        [TestMethod]
        public void Constructor_ReturnsSuccess()
        {
            // Arrange
            Mock<ILogger<BaseService>> mockLogger = new Mock<ILogger<BaseService>>();

            // Act & Assert
            var service = new BaseService(mockLogger.Object);
        }
    }
}
