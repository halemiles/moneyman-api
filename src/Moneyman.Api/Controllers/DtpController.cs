using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moneyman.Domain;
using Moneyman.Interfaces;
using Moneyman.Services.Interfaces;

namespace Moneyman.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DtpController : ControllerBase
    {
        private readonly IDtpService dtpService;
        private readonly IDtpReaderService dtpReaderService;
        private readonly ILogger<DtpController> _logger;
        private readonly IMapper _mapper;

        public DtpController(
            ILogger<DtpController> logger,
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
        public IActionResult GetCurrentPeriod(int id)
        {
            return Ok(dtpReaderService.GetCurrent());
        }

        [HttpGet("generate")]
        public IActionResult Generate([FromQuery]int? transactionId)
        {
            var planDates = dtpService.GenerateAll(transactionId);
            return Ok(planDates.Count);
            
        }
        
    }
}
