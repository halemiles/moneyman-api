using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Moneyman.Domain;
using Moneyman.Interfaces;
using System.Threading.Tasks;

namespace Moneyman.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BankAccountController : ControllerBase
    {
        private readonly IBankAccountService bankAccountService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public BankAccountController(
            ILogger logger,
            IBankAccountService bankAccountService,
            IMapper mapper
        )
        {
            _logger = logger;
            this.bankAccountService = bankAccountService;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult GetAll([FromQuery] int? bankAccountId)
        {
            _logger.Information("GET all bank accounts");
            var accounts = bankAccountService.GetAll();

            return Ok(accounts);
        }

    }
}
