using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Moneyman.Domain;
using Moneyman.Domain.Models;
using Moneyman.Domain.Models.Dtos;
using Moneyman.Interfaces;
using Moneyman.Models.Dtos;
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


        [HttpPost("current")]
        public IActionResult GetCurrentPeriod(DtpRequestDto startingValue)
        {
            _logger.Information("GET all current");
            var planDateDto = dtpService.GetCurrent(startingValue.StartingValue);
            return Ok(planDateDto);
        }

        [HttpPost("full")]
        public IActionResult GetOffsetPeriod(DtpRequestDto startingValue)
        {
            _logger.Information("GET all DTP");
            var planDateDto = dtpService.GetOffset(0);
            return Ok(planDateDto);
        }

        [HttpGet("generate")]
        public IActionResult Generate([FromQuery]int? transactionId)
        {
            _logger.Information("GET generate DTP {TransactionId}", transactionId ?? 0);

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
