using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Moneyman.Domain.Models;
using Moneyman.Services.Interfaces;
using Serilog;

namespace Moneyman.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DtpController : ControllerBase
    {
        private readonly IDtpService dtpService;
        private readonly ILogger _logger;

        public DtpController(
            ILogger logger,
            IDtpService dtpService
        )
        {
            _logger = logger;
            this.dtpService = dtpService;
        }


        [HttpGet("current")]
        public IActionResult GetCurrentPeriod(int? startingValue, int? bankAccountId)
        {
            _logger.Information("GET all current");
            var planDateDto = dtpService.GetCurrent(startingValue, bankAccountId);
            return Ok(planDateDto);
        }

        [HttpGet("full")]
        public IActionResult GetOffsetPeriod(int? startingValue, int? bankAccountId)
        {
            _logger.Information("GET all DTP");
            var planDateDto = dtpService.GetOffset(0);
            return Ok(planDateDto);
        }

        [HttpPost("generate")]
        public IActionResult Generate([FromQuery]int? transactionId)
        {
            _logger.Information("POST generate DTP {TransactionId}", transactionId ?? 0);

            try
            {
                var planDates = dtpService.GenerateAll(transactionId);
                return Ok(new DtpHttpResponse{RecordCount = planDates.Payload.Count(), Message = "Success"});
            }
            catch(Exception err)
            {
                _logger.Fatal(err.ToString());
                return Ok(new DtpHttpResponse{RecordCount = 0, Message = "Missing Paydays"});
            }
        }
    }
}
