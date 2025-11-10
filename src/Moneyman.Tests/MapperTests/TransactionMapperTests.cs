// using Microsoft.VisualStudio.TestTools.UnitTesting;
// using Moneyman.Domain;
// using System;
// using FluentAssertions;
// using Moneyman.Domain.MapperProfiles;
//
// namespace Moneyman.Tests
// {
//     [TestClass]
//     public class TransactionMapperTests
//     {
//         private TransactionMapper _mapper;
//
//         [TestInitialize]
//         public void SetUp()
//         {
//             _mapper = new TransactionMapper();
//         }
//
//         [TestMethod]
//         public void Map_TransactionToDto_ShouldMapAllProperties()
//         {
//             // Arrange
//             var transaction = new Transaction
//             {
//                 Id = 999,
//                 Name = "Test Transaction",
//                 Amount = 1234,
//                 StartDate = new DateTime(2022, 1, 1),
//                 Active = true,
//                 Frequency = Frequency.Monthly,
//                 IsAnticipated = false
//             };
//
//             // Act
//             var dto = _mapper.ToDto(transaction);
//
//             // Assert
//             dto.Id.Should().Be(transaction.Id);
//             dto.Name.Should().Be(transaction.Name);
//             dto.Amount.Should().Be(transaction.Amount);
//             dto.StartDate.Should().Be(transaction.StartDate);
//             dto.Active.Should().Be(transaction.Active);
//             dto.Frequency.Should().Be(transaction.Frequency);
//             dto.IsAnticipated.Should().Be(transaction.IsAnticipated);
//         }
//
//         [TestMethod]
//         public void Map_DtoToTransaction_ShouldMapAllProperties()
//         {
//             // Arrange
//             var dto = new TransactionDto
//             {
//                 Id = 999,
//                 Name = "Test Transaction",
//                 Amount = 1234,
//                 StartDate = new DateTime(2022, 1, 1),
//                 Active = true,
//                 Frequency = Frequency.Monthly,
//                 IsAnticipated = false
//             };
//
//             // Act
//             var transaction = _mapper.ToEntity(dto);
//
//             // Assert
//             transaction.Id.Should().Be(dto.Id);
//             transaction.Name.Should().Be(dto.Name);
//             transaction.Amount.Should().Be(dto.Amount);
//             transaction.StartDate.Should().Be(dto.StartDate);
//             transaction.Active.Should().Be(dto.Active);
//             transaction.Frequency.Should().Be(dto.Frequency);
//             transaction.IsAnticipated.Should().Be(dto.IsAnticipated);
//         }
//
//         [TestMethod]
//         public void UpdateEntity_ShouldUpdateOnlyNonNullProperties()
//         {
//             // Arrange
//             var existing = new Transaction
//             {
//                 Id = 999,
//                 Name = "Test Transaction",
//                 Amount = 1234,
//                 StartDate = new DateTime(2022, 1, 1),
//                 Active = true,
//                 Frequency = Frequency.Monthly,
//                 IsAnticipated = false
//             };
//
//             var updated = new Transaction
//             {
//                 Id = 999,
//                 Name = "Updated Name",
//                 Amount = 5678
//             };
//
//             // Act
//             _mapper.UpdateEntity(updated, existing);
//
//             // Assert
//             existing.Id.Should().Be(999);
//             existing.Name.Should().Be("Updated Name");
//             existing.Amount.Should().Be(5678);
//             existing.StartDate.Should().Be(new DateTime(2022, 1, 1));
//             existing.Active.Should().BeTrue();
//             existing.Frequency.Should().Be(Frequency.Monthly);
//             existing.IsAnticipated.Should().BeFalse();
//         }
//     }
// }
