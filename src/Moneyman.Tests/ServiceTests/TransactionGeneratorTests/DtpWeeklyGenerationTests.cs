using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moneyman.Interfaces;
using Moneyman.Services;
using Moneyman.Domain;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using System;
using Snapper;
using Microsoft.Extensions.Logging;
using AutoFixture;
using Moneyman.Services.Interfaces;
using Moneyman.Domain.MapperProfiles;

namespace Moneyman.Tests
{
    [TestClass]
    public class DtpWeeklyGenerationTests
    {
        private Mock<ITransactionRepository> mockTransactionRepository;
        private Mock<IOffsetCalculationService> mockOffsetCalculationService;
        private Mock<ILogger<DefaultPlanDateGenerationStrategy>> mockLogger;
        private DefaultPlanDateGenerationStrategy sut;

        [TestInitialize]
        public void SetUp()
        {
            mockTransactionRepository = new Mock<ITransactionRepository>();
            mockOffsetCalculationService = new Mock<IOffsetCalculationService>();
            mockLogger = new Mock<ILogger<DefaultPlanDateGenerationStrategy>>();

            mockOffsetCalculationService.Setup(x => x.CalculateOffset(It.IsAny<DateTime>()))
                .Returns((DateTime d) => new CalculatedPlanDate { PlanDate = d });

            sut = new DefaultPlanDateGenerationStrategy(
                mockTransactionRepository.Object,
                mockOffsetCalculationService.Object,
                mockLogger.Object
            );
        }

        [TestMethod]
        public void GenerateWeekly_WithEmptyTransactionList_ReturnsEmptyList()
        {
            // Arrange
            IEnumerable<Transaction> trans = new List<Transaction>();
            mockTransactionRepository.Setup(x => x.GetAll()).Returns(trans);

            // Act
            var result = sut.Generate(null, Frequency.Weekly);

            // Assert
            result.Count.Should().Be(0);
        }

        [TestMethod]
        public void GenerateWeekly_WithValidWeeklyTransaction_ReturnsSuccess()
        {
            // Arrange
            IEnumerable<Transaction> trans = new List<Transaction>
            {
                new Transaction
                {
                    Name = "Trans 1",
                    Amount = 100,
                    Active = true,
                    StartDate = new DateTime(2022,1,1),
                    Frequency = Frequency.Weekly
                }
            }.AsEnumerable();
            mockTransactionRepository.Setup(x => x.GetAll()).Returns(trans);

            // Act
            var result = sut.Generate(null, Frequency.Weekly);

            // Assert
            result.Count.Should().Be(104);
            result.All(x => x.Transaction.Name == "Trans 1").Should().BeTrue();
            result.ShouldMatchSnapshot();
        }

        [TestMethod]
        public void GenerateWeekly_WithMultipleTransactionFrequencies_ReturnsSuccess()
        {
            // Arrange
            IEnumerable<Transaction> trans = new List<Transaction>
            {
                new Transaction
                {
                    Name = "Trans 1",
                    Amount = 100,
                    Active = true,
                    StartDate = new DateTime(2022,1,1),
                    Frequency = Frequency.Monthly
                },
                new Transaction
                {
                    Name = "Trans 2",
                    Amount = 100,
                    Active = true,
                    StartDate = new DateTime(2022,1,6),
                    Frequency = Frequency.Weekly
                }
            }.AsEnumerable();
            mockTransactionRepository.Setup(x => x.GetAll()).Returns(trans);

            // Act
            var result = sut.Generate(null, Frequency.Weekly);

            // Assert
            result.Count.Should().Be(104);
            result.Any(x => x.Transaction.Name == "Trans 1").Should().BeFalse();
            result.Any(x => x.Transaction.Name == "Trans 2").Should().BeTrue();
            result.ShouldMatchSnapshot();
        }

        [TestMethod]
        public void GenerateWeekly_WithMultipleWeeklyTransactions_WhenTransactionIdSupplied_ReturnsSuccess()
        {
            // Arrange
            IEnumerable<Transaction> trans = new List<Transaction>
            {
                new Transaction
                {
                    Id = 0,
                    Name = "Trans 1",
                    Amount = 100,
                    Active = true,
                    StartDate = new DateTime(2022,1,1),
                    Frequency = Frequency.Weekly
                },
                new Transaction
                {
                    Id = 1,
                    Name = "Trans 2",
                    Amount = 100,
                    Active = true,
                    StartDate = new DateTime(2022,1,6),
                    Frequency = Frequency.Weekly
                }
            }.AsEnumerable();
            mockTransactionRepository.Setup(x => x.GetAll()).Returns(trans);

            // Act
            var result = sut.Generate(1, Frequency.Weekly);

            // Assert
            result.Count.Should().Be(104);
            result.All(x => x.Transaction.Name == "Trans 2").Should().BeTrue();
            result.ShouldMatchSnapshot();
        }
    }
}
