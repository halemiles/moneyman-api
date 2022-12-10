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
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService transactionService;
        private readonly ILogger<TransactionController> _logger;
        private readonly IMapper _mapper;

        public TransactionController(
            ILogger<TransactionController> logger,
            ITransactionService transactionService,
            IMapper mapper
        )
        {
            _logger = logger;
            this.transactionService = transactionService;
            _mapper = mapper;
        }

        [HttpPost]
        public IActionResult Create(TransactionDto transactionDto)
        {
            _logger.LogInformation("Creating transaction {TransactionName}", transactionDto?.Name);
            var transaction = _mapper.Map<TransactionDto, Transaction>(transactionDto);
            transactionService.Update(transaction);
            return Ok();
        }

        [HttpPost("multiple")]
        public IActionResult CreateMultiple([FromBody] List<TransactionDto> transactions)
        {
            _logger.LogInformation("Creating multiple transactions transaction {TransactionCount}", transactions.Count);
            var mappedTransactions = _mapper.Map<List<TransactionDto>, List<Transaction>>(transactions);
            transactionService.Update(mappedTransactions);
            return Ok();
        }

        [HttpPut]
        public IActionResult Update(TransactionDto transactionDto)
        {
            _logger.LogInformation("Updating transaction {TransactionName}", transactionDto?.Name);
            var transaction = _mapper.Map<TransactionDto, Transaction>(transactionDto);            
            transactionService.Update(transaction);
            
            return Ok(transaction); //TODO - Convert back to DTO
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            _logger.LogInformation("GET transaction {TransactionId}", id);
            var transaction = transactionService.GetById(id);
            return Ok(transaction);
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            _logger.LogInformation("GET all transactions");
            var transactions = transactionService.GetAll();
            return Ok(transactions);
        }

        

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _logger.LogInformation("DELETE transaction {TransactionId}", id);
            transactionService.Delete(id);
            return Ok();
        }
        
    }
}
