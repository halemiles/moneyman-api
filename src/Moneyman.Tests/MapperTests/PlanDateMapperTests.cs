// using Microsoft.VisualStudio.TestTools.UnitTesting;
// using Moneyman.Domain;
// using System;
// using FluentAssertions;
// using Moneyman.Domain.Mappers;
// using Moneyman.Models.Dtos;
//
// namespace Moneyman.Tests
// {
//     [TestClass]
//     public class PlanDateMapperTests
//     {
//         private PlanDateMapper _mapper;
//
//         [TestInitialize]
//         public void SetUp()
//         {
//             _mapper = new PlanDateMapper();
//         }
//
//         [TestMethod]
//         public void Map_PlanDateToDto_ShouldMapAllProperties()
//         {
//             // Arrange
//             var transaction = new Transaction
//             {
//                 Name = "Test Transaction",
//                 Amount = 100.00m,
//                 BankAccountId = 1
//             };
//
//             var planDate = new PlanDate
//             {
//                 Id = 1,
//                 Date = DateTime.Now,
//                 Transaction = transaction
//             };
//
//             // Act
//             var dto = _mapper.ToDto(planDate);
//
//             // Assert
//             dto.Id.Should().Be(planDate.Id);
//             dto.Date.Should().Be(planDate.Date);
//             dto.TransactionName.Should().Be(planDate.Transaction.Name);
//             dto.Amount.Should().Be(planDate.Transaction.Amount);
//             dto.BankAccountId.Should().Be(planDate.Transaction.BankAccountId);
//         }
//
//         [TestMethod]
//         public void Map_DtoToPlanDate_ShouldMapAllProperties()
//         {
//             // Arrange
//             var dto = new PlanDateDto
//             {
//                 Id = 1,
//                 Date = DateTime.Now,
//                 TransactionName = "Test Transaction",
//                 Amount = 100.00m,
//                 BankAccountId = 1
//             };
//
//             // Act
//             var planDate = _mapper.ToEntity(dto);
//
//             // Assert
//             planDate.Id.Should().Be(dto.Id);
//             planDate.Date.Should().Be(dto.Date);
//             planDate.Transaction.Should().NotBeNull();
//             planDate.Transaction.Name.Should().Be(dto.TransactionName);
//             planDate.Transaction.Amount.Should().Be(dto.Amount);
//             planDate.Transaction.BankAccountId.Should().Be(dto.BankAccountId);
//         }
//     }
// }
