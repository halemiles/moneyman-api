using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moneyman.Domain;
using Moneyman.Services.Interfaces;

namespace Moneyman.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PaydayController : ControllerBase
    {
        private readonly IPaydayService paydayService;
        private readonly ILogger<TransactionController> _logger;
        private readonly IMapper _mapper;

        public PaydayController(
            ILogger<TransactionController> logger,
            IPaydayService paydayService,
            IMapper mapper
        )
        {
            _logger = logger;
            this.paydayService = paydayService;
            _mapper = mapper;
        }

        [HttpGet("generate")]
        public IActionResult GeneratePaydays([FromQuery]int? dayOfMonth)
        {
            if(!dayOfMonth.HasValue || dayOfMonth == 0)
            {
                return BadRequest();
            }
            
            paydayService.Generate(dayOfMonth.Value);
            return Ok();
        }
    }
}
