using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moneyman.Models;
using Moneyman.Services.Validators;


namespace Moneyman.Tests
{
    [TestClass]
    public class PaydayDtoValidatorTests
    {
        private PaydayDtoValidator _validator;

        [TestInitialize]
        public void Setup()
        {
            _validator = new PaydayDtoValidator();
        }

        [TestMethod]
        public void GivenValidPaydayDto_ValidationShouldPass()
        {
            // Arrange
            var paydayDto = new PaydayDto { DayOfMonth = 15 };

            // Act
            var result = _validator.Validate(paydayDto);

            // Assert
            result.IsValid.Should().BeTrue();
        }

        [TestMethod]
        [Ignore]
        public void GivenInvalidPaydayDtoWithNullDayOfMonth_ValidationShouldFail()
        {
            // Arrange
            var paydayDto = new PaydayDto { DayOfMonth = null };

            // Act
            var result = _validator.Validate(paydayDto);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Errors.Should().ContainSingle();
            result.Errors[0].ErrorMessage.Should().Be("'Day Of Month' must not be empty.");
        }
    }
}