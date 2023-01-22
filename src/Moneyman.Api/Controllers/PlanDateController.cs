using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moneyman.Domain;
using Moneyman.Interfaces;

namespace Moneyman.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PlandateController : ControllerBase
    {
        private readonly IPlanDateService planDateService;
        private readonly ILogger<PlandateController> _logger;
        private readonly IMapper _mapper;

        public PlandateController(
            ILogger<PlandateController> logger,
            IPlanDateService planDateService,
            IMapper mapper
        )
        {
            _logger = logger;
            this.planDateService = planDateService;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            _logger.LogInformation("Getting all plan dates");
            var planDates = planDateService.GetAll();
            return Ok(planDates);
        }

        [HttpGet("search")]
        public IActionResult Search(string transactionName)
        {
            _logger.LogInformation("Getting all plan dates");
            var planDates = planDateService.Search(transactionName);
            return Ok(planDates);
        }
    }
}
