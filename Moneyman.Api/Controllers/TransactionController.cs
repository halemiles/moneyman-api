using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moneyman.Domain;
using Moneyman.Interfaces;

namespace Moneyman.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TransactionController : ControllerBase
    {
        ITransactionService transactionService;

        private readonly ILogger<TransactionController> _logger;

        public TransactionController(ILogger<TransactionController> logger, ITransactionService transactionService)
        {
            _logger = logger;
            this.transactionService = transactionService;
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var transaction = transactionService.GetById(id);
            return Ok(transaction);
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var transactions = transactionService.GetAll();
            return Ok(transactions);
        }

        [HttpPost]
        public IActionResult Create(Transaction trans)
        {
            transactionService.Update(trans,0);
            return Ok();
        }
    }
}
