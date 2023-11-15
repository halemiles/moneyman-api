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
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService transactionService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public TransactionController(
            ILogger logger,
            ITransactionService transactionService,
            IMapper mapper
        )
        {
            _logger = logger;
            this.transactionService = transactionService;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<IActionResult> Create(TransactionDto transactionDto)
        {
            _logger.Information("Creating transaction {TransactionName}", transactionDto?.Name);
            
            var result = await transactionService.Create(transactionDto);
            if(!result.Success)
            {
                return StatusCode((int)result.StatusCode);
            }
            return Ok(result);
        }

        [HttpPost("multiple")]
        public IActionResult CreateMultiple([FromBody] List<TransactionDto> transactions)
        {
            _logger.Information("Creating multiple transactions transaction {TransactionCount}", transactions.Count);
            var mappedTransactions = _mapper.Map<List<TransactionDto>, List<Transaction>>(transactions);
            transactionService.Update(mappedTransactions);
            return Ok();
        }

        [HttpPut]
        public IActionResult Update(TransactionDto transactionDto)
        {
            _logger.Information("Updating transaction {TransactionName}", transactionDto?.Name);
            var transaction = _mapper.Map<TransactionDto, Transaction>(transactionDto);            
            transactionService.Update(transaction);
            
            return Ok(transaction); //TODO - Convert back to DTO
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            _logger.Information("GET transaction {TransactionId}", id);
            var transaction = transactionService.GetById(id);
            return Ok(transaction);
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            _logger.Information("GET all transactions");
            var transactions = transactionService.GetAll();
            return Ok(transactions);
        }

        

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _logger.Information("DELETE transaction {TransactionId}", id);
            transactionService.Delete(id);
            return Ok();
        }
        
    }
}
