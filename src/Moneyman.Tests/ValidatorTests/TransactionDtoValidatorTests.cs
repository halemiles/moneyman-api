using System;
using System.Linq;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moneyman.Domain;
using Moneyman.Services.Validators;

namespace Moneyman.Tests
{

    [TestClass]
    public class TransactionDtoValidatorTests
    {
        private TransactionDtoValidator _validator;

        [TestInitialize]
        public void Setup()
        {
            _validator = new TransactionDtoValidator();
        }

        [TestMethod]
        public void ShouldHaveErrorWhenNameIsNull()
        {
            // Arrange
            var dto = new TransactionDto { Name = null, Amount = 100, Date = DateTime.Now };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [TestMethod]
        public void ShouldHaveErrorWhenNameIsEmpty()
        {
            // Arrange
            var dto = new TransactionDto { Name = "", Amount = 100, Date = DateTime.Now };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [TestMethod]
        public void ShouldHaveErrorWhenAmountIsZero()
        {
            // Arrange
            var dto = new TransactionDto { Name = "Test", Amount = 0, Date = DateTime.Now };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Amount);
        }

        [TestMethod]
        public void ShouldHaveErrorWhenAmountIsNull()
        {
            // Arrange
            var dto = new TransactionDto { Name = "Test", Amount = null, Date = DateTime.Now };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Amount);
        }

        [TestMethod]
        public void ShouldHaveErrorWhenDateIsMinValue()
        {
            // Arrange
            var dto = new TransactionDto { Name = "Test", Amount = 100, Date = DateTime.MinValue };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Date);
        }

        [TestMethod]
        public void ShouldNotHaveErrorsWhenAllFieldsAreValid()
        {
            // Arrange
            var dto = new TransactionDto { Name = "Test", Amount = 100, Date = DateTime.Now };

            // Act
            var result = _validator.TestValidate(dto);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}