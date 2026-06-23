using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moneyman.Domain;
using System;
using System.Collections.Generic;
using FluentAssertions;
using Moneyman.Domain.MapperProfiles;
using Moneyman.Models.DomainTransferObjects;

namespace Moneyman.Tests
{
    [TestClass]
    public class PlanDateMapperTests
    {
        private PlanDateMapper _mapper;

        [TestInitialize]
        public void SetUp()
        {
            _mapper = new PlanDateMapper();
        }

        [TestMethod]
        public void ToDto_WithPlanDate_MapsTransactionProperties()
        {
            // Arrange
            var date = new DateTime(2022, 10, 20);
            var transaction = new Transaction
            {
                Name = "Test Transaction",
                Amount = 100.00m,
                BankAccountId = 1
            };
            var planDate = new PlanDate
            {
                Id = 1,
                Date = date,
                Transaction = transaction
            };

            // Act
            var dto = _mapper.ToDto(planDate);

            // Assert
            dto.Date.Should().Be(date);
            dto.TransactionName.Should().Be(transaction.Name);
            dto.Amount.Should().Be(transaction.Amount);
            dto.BankAccountId.Should().Be(transaction.BankAccountId);
        }

        [TestMethod]
        public void ToDto_WhenTransactionIsNull_ReturnsNull()
        {
            // Arrange
            var planDate = new PlanDate { Date = new DateTime(2022, 10, 20), Transaction = null };

            // Act
            var dto = _mapper.ToDto(planDate);

            // Assert
            dto.Should().BeNull();
        }

        [TestMethod]
        public void ToDto_WhenPlanDateIsNull_ReturnsNull()
        {
            // Act
            var dto = _mapper.ToDto(null);

            // Assert
            dto.Should().BeNull();
        }

        [TestMethod]
        public void ToEntity_WithDto_MapsTransactionProperties()
        {
            // Arrange
            var date = new DateTime(2022, 10, 20);
            var dto = new PlanDateDto
            {
                Date = date,
                TransactionName = "Test Transaction",
                Amount = 100.00m,
                BankAccountId = 1
            };

            // Act
            var planDate = _mapper.ToEntity(dto);

            // Assert
            planDate.Date.Should().Be(date);
            planDate.Transaction.Should().NotBeNull();
            planDate.Transaction.Name.Should().Be(dto.TransactionName);
            planDate.Transaction.Amount.Should().Be(dto.Amount);
            planDate.Transaction.BankAccountId.Should().Be(dto.BankAccountId);
        }

        [TestMethod]
        public void ToEntity_WhenDtoIsNull_ReturnsNull()
        {
            // Act
            var planDate = _mapper.ToEntity(null);

            // Assert
            planDate.Should().BeNull();
        }

        [TestMethod]
        public void ToDtoList_WithPlanDates_MapsEachItem()
        {
            // Arrange
            var planDates = new List<PlanDate>
            {
                new PlanDate { Date = new DateTime(2022, 1, 1), Transaction = new Transaction { Name = "A", Amount = 1m, BankAccountId = 1 } },
                new PlanDate { Date = new DateTime(2022, 2, 1), Transaction = new Transaction { Name = "B", Amount = 2m, BankAccountId = 2 } }
            };

            // Act
            var dtos = _mapper.ToDtoList(planDates);

            // Assert
            dtos.Should().HaveCount(2);
            dtos[0].TransactionName.Should().Be("A");
            dtos[1].TransactionName.Should().Be("B");
        }
    }
}
