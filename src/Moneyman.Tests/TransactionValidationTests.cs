using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moneyman.Interfaces;
using Moneyman.Services;

using Moneyman.Services.Validators;
using Moneyman.Domain;
using FluentAssertions;
using System.Linq;
using System;
using Snapper;

namespace Moneyman.Tests
{
    [TestClass]
    public class TransactionValidationTests
    {
        private Mock<ITransactionService> mockTransactionService;

        private TransactionDtoValidator NewTransactionValidator() => 
                new TransactionDtoValidator();

        [TestInitialize]
        public void SetUp()
        {
            mockTransactionService = new Mock<ITransactionService>();
        }

        [TestMethod]
        public void Create_WithValidProperties_ReturnsSuccess()
        {
            var sut = NewTransactionValidator();
            TransactionDto trans = new TransactionDto
            {
                Name = "Transaction 1",
                Amount = 100,
                Date = DateTime.Today

            };
            var result = sut.Validate(trans);

            result.IsValid.Should().Be(true);
        }

        [TestMethod]
        public void Create_WithNullName_ReturnsFailure()
        {
            var sut = NewTransactionValidator();
            TransactionDto trans = new TransactionDto
            {
                Name = null
            };
            var result = sut.Validate(trans);

            result.IsValid.Should().Be(false);
            result.Errors.ShouldMatchSnapshot();
        }

        [TestMethod]
        public void Create_WithEmptyName_ReturnsFailure()
        {
            var sut = NewTransactionValidator();
            TransactionDto trans = new TransactionDto
            {
                Name = string.Empty

            };
            var result = sut.Validate(trans);

            result.IsValid.Should().Be(false);
            result.Errors.ShouldMatchSnapshot();
        }

        [TestMethod]
        public void Create_WithZeroAmount_ReturnsFailure()
        {
            var sut = NewTransactionValidator();
            TransactionDto trans = new TransactionDto
            {
                Name = "Transaction 1",
                Amount = 0

            };
            var result = sut.Validate(trans);

            result.IsValid.Should().Be(false);
            result.Errors.ShouldMatchSnapshot();
        }

        [TestMethod]
        public void Create_WithMinDateTime_ReturnsFailure()
        {
            var sut = NewTransactionValidator();
            TransactionDto trans = new TransactionDto
            {
                Name = "Transaction 1",
                Amount = 500,
                Date = DateTime.MinValue

            };
            var result = sut.Validate(trans);

            result.IsValid.Should().Be(false);
            result.Errors.ShouldMatchSnapshot();
        }
    }
}
