using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moneyman.Interfaces;
using Moneyman.Services;
using AutoMapper;
using System.Linq;
using FluentAssertions;

namespace Moneyman.Tests
{
    [TestClass]
    public class PaydayServiceTests
    {
        private Mock<IPaydayRepository> mockPaydayRepository;
        private IMapper _mapper;
        private PaydayService NewPaydayService() 
            => new PaydayService(mockPaydayRepository.Object);

        [TestInitialize]
        public void SetUp()
        {
            mockPaydayRepository = new Mock<IPaydayRepository>();
        }

        [TestMethod]
        public void Create_WithValidDetails_ReturnsSuccess()
        {
            int dayOfMonth = 25;
            var paydayService = NewPaydayService();
            var result = paydayService.Generate(dayOfMonth);

            result.Count.Should().Be(12);
            result.FirstOrDefault().Date.Month.Should().Be(1);
            result.LastOrDefault().Date.Month.Should().Be(12);
        }
    }
}
