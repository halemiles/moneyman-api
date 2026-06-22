using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
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

        public PlandateController(
            ILogger<PlandateController> logger,
            IPlanDateService planDateService
        )
        {
            _logger = logger;
            this.planDateService = planDateService;
        }

        [HttpGet]
        [ProducesResponseType<List<PlanDate>>(StatusCodes.Status200OK)]
        public IActionResult GetAll()
        {
            _logger.LogInformation("Getting all plan dates");
            var planDates = planDateService.GetAll();
            return Ok(planDates);
        }

        [HttpGet("search")]
        [ProducesResponseType<List<PlanDate>>(StatusCodes.Status200OK)]
        public IActionResult Search(string transactionName)
        {
            _logger.LogInformation("Getting all plan dates");
            var planDates = planDateService.Search(transactionName);
            return Ok(planDates);
        }

        [HttpPatch("{id}/paid")]
        [ProducesResponseType<ApiResponse<PlanDate>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ApiResponse<PlanDate>>(StatusCodes.Status404NotFound)]
        public IActionResult MarkAsPaid(int id)
        {
            _logger.LogInformation("Marking plan date {Id} as paid", id);
            var result = planDateService.MarkAsPaid(id);
            if (result.StatusCode == Domain.StatusCode.NotFound)
            {
                return NotFound(result);
            }
            return Ok(result);
        }
    }
}
