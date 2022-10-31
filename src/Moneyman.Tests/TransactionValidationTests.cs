using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moneyman.Interfaces;
using Moneyman.Services;

using Moneyman.Services.Validators;
using Moneyman.Domain;
using FluentAssertions;
using System.Linq;
using System;

namespace Moneyman.Tests
{
    [TestClass]
    public class TransactionValidationTests
    {
        private Mock<ITransactionService> mockTransactionService;

        private TransactionValidator NewTransactionValidator() => 
                new TransactionValidator();

        [TestInitialize]
        public void SetUp()
        {
            mockTransactionService = new Mock<ITransactionService>();
        }

        [TestMethod]
        public void Create_WithValidProperties_ReturnsSuccess()
        {
            var sut = NewTransactionValidator();
            Transaction trans = new Transaction
            {
                Name = "Transaction 1",
                Amount = 100,
                StartDate = DateTime.Today

            };
            var result = sut.Validate(trans);

            result.IsValid.Should().Be(true);
        }

        [TestMethod]
        public void Create_WithNullName_ReturnsFailure()
        {
            var sut = NewTransactionValidator();
            Transaction trans = new Transaction
            {
                Name = null
            };
            var result = sut.Validate(trans);

            result.IsValid.Should().Be(false);
            result.Errors.Count.Should().Be(4);
            result.Errors.FirstOrDefault().PropertyName.Should().Be("Name");
        }

        [TestMethod]
        public void Create_WithEmptyName_ReturnsFailure()
        {
            var sut = NewTransactionValidator();
            Transaction trans = new Transaction
            {
                Name = string.Empty

            };
            var result = sut.Validate(trans);

            result.IsValid.Should().Be(false);
            result.Errors.Count.Should().Be(3);
            result.Errors.FirstOrDefault().PropertyName.Should().Be("Name");
        }

        [TestMethod]
        public void Create_WithZeroAmount_ReturnsFailure()
        {
            var sut = NewTransactionValidator();
            Transaction trans = new Transaction
            {
                Name = "Transaction 1",
                Amount = 0

            };
            var result = sut.Validate(trans);

            result.IsValid.Should().Be(false);
            result.Errors.Count.Should().Be(2);
            result.Errors.FirstOrDefault().PropertyName.Should().Be("Amount");
        }

        [TestMethod]
        public void Create_WithMinDateTime_ReturnsFailure()
        {
            var sut = NewTransactionValidator();
            Transaction trans = new Transaction
            {
                Name = "Transaction 1",
                Amount = 500,
                StartDate = DateTime.MinValue

            };
            var result = sut.Validate(trans);

            result.IsValid.Should().Be(false);
            result.Errors.Count.Should().Be(1);
            result.Errors.FirstOrDefault().PropertyName.Should().Be("StartDate");
        }
    }
}
