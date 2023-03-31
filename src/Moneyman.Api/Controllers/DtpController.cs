using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Moneyman.Domain;
using Moneyman.Domain.Models;
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
        private readonly IDtpReaderService dtpReaderService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public DtpController(
            ILogger logger,
            IDtpService dtpService,
            IDtpReaderService dtpReaderService,
            IMapper mapper
        )
        {
            _logger = logger;
            this.dtpService = dtpService;
            this.dtpReaderService = dtpReaderService;
            _mapper = mapper;
        }

        
        [HttpGet("current")]
        public IActionResult GetCurrentPeriod()
        {
            _logger.Information("GET all current");
            var planDateDto = dtpReaderService.GetCurrent();
            return Ok(planDateDto);            
        }

        [HttpGet("full")]
        public IActionResult GetOffsetPeriod(int? monthOffset = 0)
        {
            _logger.Information("GET all DTP");
            var planDateDto = dtpReaderService.GetOffset(monthOffset.Value);
            return Ok(planDateDto);            
        }

        [HttpGet("generate")]
        public IActionResult Generate([FromQuery]int? transactionId)
        {
            _logger.Information("GET generate DTP {TransactionId}", transactionId ?? 0);

            try
            {
                var planDates = dtpService.GenerateAll(transactionId);
                return Ok(); //(new DtpHttpResponse{RecordCount = 0, Message = "Success"});
            }
            catch(Exception err)
            {
                return Ok(new DtpHttpResponse{RecordCount = 0, Message = "Missing Paydays"});
            }

            
            
        }
        
    }
}
