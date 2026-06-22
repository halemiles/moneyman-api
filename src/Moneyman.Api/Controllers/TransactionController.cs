// src/Moneyman.Api/Controllers/TransactionController.cs
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Moneyman.Domain;
using Moneyman.Domain.MapperProfiles;
using Moneyman.Interfaces;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Moneyman.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService transactionService;
        private readonly ILogger<TransactionController> _logger;
        private readonly TransactionMapper mapper;

        public TransactionController(
            ILogger<TransactionController> logger,
            ITransactionService transactionService,
            TransactionMapper mapper
        )
        {
            _logger = logger;
            this.transactionService = transactionService;
            this.mapper = mapper;
        }

        [HttpPost]
        public async Task<IActionResult> Create(TransactionDto transactionDto)
        {
            _logger.LogInformation("Creating transaction {TransactionName}", transactionDto?.Name);

            var result = await transactionService.Create(transactionDto);
            if(!result.Success)
            {
                return StatusCode((int)result.StatusCode, new { message = result.Message });
            }
            return Ok(new { id = result.Payload });
        }

        [HttpPost("multiple")]
        public IActionResult CreateMultiple([FromBody] List<TransactionDto> transactions)
        {
            _logger.LogInformation("Creating multiple transactions transaction {TransactionCount}", transactions.Count);
            var mappedTransactions = mapper.ToEntityList(transactions);
            transactionService.Update(mappedTransactions);
            return Ok();
        }

        [HttpPut]
        public IActionResult Update(TransactionDto transactionDto)
        {
            _logger.LogInformation("Updating transaction {TransactionName}", transactionDto?.Name);
            var transaction = mapper.ToEntity(transactionDto);
            var result = transactionService.Update(transaction);

            if (result == 0)
            {
                return NotFound();
            }

            return Ok(mapper.ToDto(transaction));
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            _logger.LogInformation("GET transaction {TransactionId}", id);
            var transaction = transactionService.GetById(id);

            if(transaction == null)
            {
                return NotFound();
            }
            return Ok(transaction);
        }

        [HttpGet]
        public IActionResult GetAll([FromQuery] bool? anticipated, [FromQuery] int? bankAccountId)
        {
            _logger.LogInformation("GET all transactions");
            var transactions = transactionService.GetAll();

            if (anticipated.HasValue && anticipated.Value)
            {
                transactions = transactions.Where(x => x.Frequency == Frequency.Anticipated).ToList();
            }
            else
            {
                transactions = transactions.Where(x => x.Frequency != Frequency.Anticipated).ToList();
            }

            if (bankAccountId.HasValue)
            {
                transactions = transactions.Where(x => x.BankAccountId == bankAccountId.Value).ToList();
            }

            return Ok(transactions);
        }



        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _logger.LogInformation("DELETE transaction {TransactionId}", id);
            transactionService.Delete(id);
            return Ok();
        }

        [HttpDelete("all")]
        public IActionResult DeleteAll()
        {
            _logger.LogInformation("DELETE all transactions");
            transactionService.DeleteAll();
            return Ok();
        }

    }
}
