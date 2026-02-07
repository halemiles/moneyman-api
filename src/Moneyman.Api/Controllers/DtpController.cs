using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moneyman.Domain.Models;
using Moneyman.Services.Interfaces;

namespace Moneyman.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DtpController : ControllerBase
    {
        private readonly IDtpService dtpService;
        private readonly ILogger<DtpController> _logger;

        public DtpController(
            ILogger<DtpController> logger,
            IDtpService dtpService
        )
        {
            _logger = logger;
            this.dtpService = dtpService;
        }


        [HttpGet("current")]
        public IActionResult GetCurrentPeriod(int? startingValue, int? bankAccountId)
        {
            _logger.LogInformation("GET all current");
            var planDateDto = dtpService.GetCurrent(startingValue, bankAccountId);
            return Ok(planDateDto);
        }

        [HttpGet("full")]
        public IActionResult GetOffsetPeriod(int? startingValue, int? bankAccountId)
        {
            _logger.LogInformation("GET all DTP");
            var planDateDto = dtpService.GetOffset(0);
            return Ok(planDateDto);
        }

        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromQuery]int? transactionId)
        {
            _logger.LogInformation("GET generate DTP {TransactionId}", transactionId ?? 0);

            try
            {
                var planDates = await dtpService.GenerateAll(transactionId);

                if (!planDates.Success)
                {
                    return Ok(new DtpHttpResponse{RecordCount = 0, Message = planDates.Message});
                }

                return Ok(new DtpHttpResponse{RecordCount = planDates.Payload.Count(), Message = planDates.Message});
            }
            catch(Exception err)
            {
                _logger.LogError(err.ToString());
                return Ok(new DtpHttpResponse{RecordCount = 0, Message = $"Unexpected error occurred: {err.Message}"});
            }
        }

    }
}
