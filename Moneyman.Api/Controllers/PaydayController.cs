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
    public class PaydayController : ControllerBase
    {
        private IPaydayService paydayService;

        private readonly ILogger<TransactionController> _logger;
        private readonly IMapper _mapper;

        public PaydayController(
            ILogger<TransactionController> logger,
            IPaydayService paydayService,
            IMapper mapper
        )
        {
            _logger = logger;
            this.paydayService = paydayService;
            _mapper = mapper;
        }

        [HttpPost("generate")]
        public IActionResult GeneratePaydays(TransactionDto transactionDto)
        {
            paydayService.Generate(25);
            return Ok();
        }
    }
}
