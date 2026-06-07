using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.AspNetCore.Mvc;
using Moneyman.Domain;
using Moneyman.Interfaces;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Moneyman.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BankAccountController : ControllerBase
    {
        private readonly IBankAccountService bankAccountService;
        private readonly ILogger<BankAccountController> _logger;

        public BankAccountController(
            ILogger<BankAccountController> logger,
            IBankAccountService bankAccountService
        )
        {
            _logger = logger;
            this.bankAccountService = bankAccountService;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            _logger.LogInformation("GET all bank accounts");
            var accounts = bankAccountService.GetAll();

            return Ok(accounts);
        }

    }
}
