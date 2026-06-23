using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moneyman.Domain;
using FluentAssertions;

namespace Moneyman.Tests
{
    [TestClass]
    public class BankAccountMapperTests
    {
        private BankAccountMapper _mapper;

        [TestInitialize]
        public void SetUp()
        {
            _mapper = new BankAccountMapper();
        }

        [TestMethod]
        public void ToDto_WithBankAccount_MapsAllProperties()
        {
            // Arrange
            var bankAccount = new BankAccount
            {
                Id = 1,
                Name = "Test Account"
            };

            // Act
            var dto = _mapper.ToDto(bankAccount);

            // Assert
            dto.Id.Should().Be(bankAccount.Id);
            dto.Name.Should().Be(bankAccount.Name);
        }

        [TestMethod]
        public void ToEntity_WithDto_MapsAllProperties()
        {
            // Arrange
            var dto = new BankAccountDto
            {
                Id = 1,
                Name = "Test Account"
            };

            // Act
            var bankAccount = _mapper.ToEntity(dto);

            // Assert
            bankAccount.Id.Should().Be(dto.Id);
            bankAccount.Name.Should().Be(dto.Name);
        }
    }
}
