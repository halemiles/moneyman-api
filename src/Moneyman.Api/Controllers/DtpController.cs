using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moneyman.Domain;
using Moneyman.Domain.Models;
using Moneyman.Models.DomainTransferObjects;
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
        [ProducesResponseType<ApiResponse<DtpDto>>(StatusCodes.Status200OK)]
        public IActionResult GetCurrentPeriod(int? startingValue, [FromQuery] int? bankAccountId)
        {
            _logger.LogInformation("GET all current");
            var planDateDto = dtpService.GetCurrent(startingValue, bankAccountId);
            return Ok(planDateDto);
        }

        [HttpGet("full")]
        [ProducesResponseType<ApiResponse<DtpDto>>(StatusCodes.Status200OK)]
        public IActionResult GetOffsetPeriod([FromQuery] int? bankAccountId)
        {
            _logger.LogInformation("GET all DTP");
            var planDateDto = dtpService.GetOffset(0, bankAccountId);
            return Ok(planDateDto);
        }

        [HttpGet("all")]
        [ProducesResponseType<ApiResponse<DtpDto>>(StatusCodes.Status200OK)]
        public IActionResult GetAllPeriods([FromQuery] int? bankAccountId)
        {
            _logger.LogInformation("GET all DTP (full span)");
            var planDateDto = dtpService.GetAll(bankAccountId);
            return Ok(planDateDto);
        }

        [HttpPost("generate")]
        [ProducesResponseType<DtpHttpResponse>(StatusCodes.Status200OK)]
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
                _logger.LogError(err, "Unexpected error generating DTP");
                return Ok(new DtpHttpResponse{RecordCount = 0, Message = $"Unexpected error occurred: {err.Message}"});
            }
        }

    }
}
