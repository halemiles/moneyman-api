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
    public class DashboardController : ControllerBase
    {
        private readonly IDtpService dtpService;
        private readonly ILogger _logger;

        public DashboardController(
            ILogger logger,
            IDtpService dtpService
        )
        {
            _logger = logger;
            this.dtpService = dtpService;
        }


        [HttpGet("threedays")]
        public IActionResult GetThreeDays(int? bankAccountId)
        {
            _logger.Information("GET all current");
            var planDates = dtpService.GetOffset(0).Payload.PlanDates;
            var previousDate = planDates.Where(x => x.Date < DateTime.Now).FirstOrDefault();
            var previousTransactions = planDates.Where(x => x.Date == previousDate.Date).ToList();

            var todaysTransactions = planDates.Where(x => x.Date == DateTime.Now).ToList();

            var nextDate = planDates.Where(x => x.Date > DateTime.Now).FirstOrDefault();
            var nextTransactions = planDates.Where(x => x.Date == nextDate.Date).ToList();

            return Ok(new {previousDates=previousTransactions, todaysTransactions, nextDates=nextTransactions});
        }
    }
}
