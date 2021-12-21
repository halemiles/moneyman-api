using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moneyman.Domain;
using Api.Interfaces;

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

        [HttpPost]
        public IEnumerable<Transaction> Create(Transaction trans)
        {
            transactionService.Update(trans,0);
            return new List<Transaction>();
        }
    }
}
