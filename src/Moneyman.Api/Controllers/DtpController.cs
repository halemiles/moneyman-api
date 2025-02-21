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

    }
}
