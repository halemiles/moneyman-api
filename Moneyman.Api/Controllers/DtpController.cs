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
        IDtpService dtpService;

        private readonly ILogger<DtpController> _logger;
        private readonly IMapper _mapper;

        public DtpController(
            ILogger<DtpController> logger,
            IDtpService dtpService,
            IMapper mapper
        )
        {
            _logger = logger;
            this.dtpService = dtpService;
            _mapper = mapper;
        }

        
        [HttpGet("current")]
        public IActionResult GetCurrentPeriod(int id)
        {
            return Ok();
        }

        [HttpGet("generate")]
        public IActionResult Generate()
        {
            var planDates = dtpService.GenerateAll();
            return Ok(planDates.Count);
            
        }
        
    }
}
