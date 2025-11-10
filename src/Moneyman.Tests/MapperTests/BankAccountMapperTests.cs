// using Microsoft.VisualStudio.TestTools.UnitTesting;
// using Moneyman.Domain;
// using System;
// using FluentAssertions;
// using Moneyman.Domain.Mappers;
// using BankAccountMapper = Moneyman.Domain.BankAccountMapper;
//
// namespace Moneyman.Tests
// {
//     [TestClass]
//     public class BankAccountMapperTests
//     {
//         private BankAccountMapper _mapper;
//
//         [TestInitialize]
//         public void SetUp()
//         {
//             _mapper = new BankAccountMapper();
//         }
//
//         [TestMethod]
//         public void Map_BankAccountToDto_ShouldMapAllProperties()
//         {
//             // Arrange
//             var bankAccount = new BankAccount
//             {
//                 Id = 1,
//                 Name = "Test Account",
//                 Balance = 1000.00m,
//                 IsDefault = true
//             };
//
//             // Act
//             var dto = _mapper.ToDto(bankAccount);
//
//             // Assert
//             dto.Id.Should().Be(bankAccount.Id);
//             dto.Name.Should().Be(bankAccount.Name);
//             dto.Balance.Should().Be(bankAccount.Balance);
//             dto.IsDefault.Should().Be(bankAccount.IsDefault);
//         }
//
//         [TestMethod]
//         public void Map_DtoToBankAccount_ShouldMapAllProperties()
//         {
//             // Arrange
//             var dto = new BankAccountDto
//             {
//                 Id = 1,
//                 Name = "Test Account",
//                 Balance = 1000.00m,
//                 IsDefault = true
//             };
//
//             // Act
//             var bankAccount = _mapper.ToEntity(dto);
//
//             // Assert
//             bankAccount.Id.Should().Be(dto.Id);
//             bankAccount.Name.Should().Be(dto.Name);
//             bankAccount.Balance.Should().Be(dto.Balance);
//             bankAccount.IsDefault.Should().Be(dto.IsDefault);
//         }
//     }
// }
