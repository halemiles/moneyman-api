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
        ITransactionService transactionService;

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
            var transaction = _mapper.Map<TransactionDto, Transaction>(transactionDto);
            transactionService.Update(transaction);
            return Ok();
        }

        [HttpPut]
        public IActionResult Update(TransactionDto transactionDto)
        {
            var transaction = _mapper.Map<TransactionDto, Transaction>(transactionDto);            
            transactionService.Update(transaction);
            
            return Ok(transaction); //TODO - Convert back to DTO
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

        

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            transactionService.Delete(id);
            return Ok();
        }
        
    }
}
