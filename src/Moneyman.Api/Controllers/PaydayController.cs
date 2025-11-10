
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moneyman.Domain;
using Moneyman.Models;
using Moneyman.Services.Interfaces;
using Moneyman.Services.Validators;

namespace Moneyman.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PaydayController : ControllerBase
    {
        private readonly IPaydayService paydayService;
        private readonly ILogger<PaydayController> _logger;

        public PaydayController(
            ILogger<PaydayController> logger,
            IPaydayService paydayService
        )
        {
            _logger = logger;
            this.paydayService = paydayService;
        }

        [HttpPost("generate")]
        public IActionResult GeneratePaydays([FromBody]PaydayDto body)
        {
            _logger.LogError("Generating paydays {month}", body.DayOfMonth);
             PaydayDtoValidator transactionValidator = new PaydayDtoValidator();
            if(!transactionValidator.Validate(body).IsValid)
            {
                return BadRequest();
            }
            
            var paydays = paydayService.Generate(body.DayOfMonth.Value);
            return Ok(paydays);
        }
        
        [HttpDelete("clear")]
        public IActionResult ClearPaydays()
        {
            
            paydayService.RemoveAll();
            return Ok();
        }

        [HttpGet]
        public IActionResult GetAllPaydays()
        {
            var payitys = paydayService.GetAll();
            return Ok(payitys);
        }
    }
}
