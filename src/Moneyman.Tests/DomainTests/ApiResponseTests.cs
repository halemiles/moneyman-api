using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moneyman.Domain.Models;
using Moq;

namespace Moneyman.UnitTests
{
    [TestClass]
    public class ApiResponseTests
    {
        [TestMethod]
        public void ApiResponse_Success_ReturnsSuccessResponseWithPayload()
        {
            // Arrange
            var payload = new { Name = "John", Age = 30 };
            var expectedResponse = new ApiResponse<object>(StatusCode.Success, "Success", payload);

            // Act
            var actualResponse = payload.Success();

            // Assert
            Assert.AreEqual(expectedResponse.Success, actualResponse.Success);
            Assert.AreEqual(expectedResponse.StatusCode, actualResponse.StatusCode);
            Assert.AreEqual(expectedResponse.Message, actualResponse.Message);
            Assert.AreEqual(expectedResponse.Payload, actualResponse.Payload);
        }

        [TestMethod]
        public void ApiResponse_ValidationError_ReturnsValidationErrorResponse()
        {
            // Arrange
            var expectedResponse = new ApiResponse<object>(StatusCode.BadRequest, "Validation error", null);

            // Act
            var actualResponse = ApiResponse.ValidationError<object>("Validation error");

            // Assert
            Assert.AreEqual(expectedResponse.Success, actualResponse.Success);
            Assert.AreEqual(expectedResponse.StatusCode, actualResponse.StatusCode);
            Assert.AreEqual(expectedResponse.Message, actualResponse.Message);
            Assert.AreEqual(expectedResponse.Payload, actualResponse.Payload);
        }

        [TestMethod]
        public void ApiResponse_NotFound_ReturnsNotFoundResponse()
        {
            // Arrange
            var expectedResponse = new ApiResponse<object>(StatusCode.NotFound, "Not found", null);

            // Act
            var actualResponse = ApiResponse.NotFound<object>("Not found");

            // Assert
            Assert.AreEqual(expectedResponse.Success, actualResponse.Success);
            Assert.AreEqual(expectedResponse.StatusCode, actualResponse.StatusCode);
            Assert.AreEqual(expectedResponse.Message, actualResponse.Message);
            Assert.AreEqual(expectedResponse.Payload, actualResponse.Payload);
        }
    }
}