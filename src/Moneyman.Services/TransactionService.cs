using Moneyman.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System;
using Moneyman.Domain;
using Moneyman.Services.Validators;
using AutoMapper;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moneyman.Domain.Models;

namespace Moneyman.Services
{
	public class TransactionService : ITransactionService
	{
		private readonly ITransactionRepository _transactionRepository;
    private readonly ILogger<TransactionService> logger;

    private readonly IMapper mapper;

		public TransactionService(
      ITransactionRepository transactionRepository,
      ILogger<TransactionService> logger,
      IMapper mapper
    )
		{
			_transactionRepository = transactionRepository;
      this.logger = logger;
      this.mapper = mapper;
		}

    public int Update(Transaction model)
    {
       
      _transactionRepository.Update(model);
      logger.LogInformation("Saving transaction {TransactionName}", model.Name);
      _transactionRepository.Save();
     

      return model.Id;
    }

    public void Update(List<Transaction> model)
    {
      foreach(var transaction in model)
      {
        _transactionRepository.Update(transaction);
      }
      logger.LogInformation("Saving transactions {TransactionCount}", model.Count);
      _transactionRepository.Save();
    }

    public void Delete(int id)
    {
        _transactionRepository.Remove(id);
        logger.LogInformation("Deleting transaction {TransactionId}", id); //TODO - Add transaction name to log?
        _transactionRepository.Save();
    }

    public List<TransactionDto> GetAll()
    {
      var transactions =  _transactionRepository.GetAll();
      var transactionsAsDto = mapper.Map<List<TransactionDto>>(transactions);
      return transactionsAsDto?.ToList() ?? new List<TransactionDto>();
    }

    public Transaction GetById(int id)
    {
      return _transactionRepository.Get(id);
    }

    public async Task<ApiResponse<int>> Create(TransactionDto trans)
    {
		
      TransactionDtoValidator transactionValidator = new TransactionDtoValidator();
      logger.LogInformation("Validation transaction {TransactionName}", trans.Name);
      var validationResult = transactionValidator.Validate(trans);
      var transaction = mapper.Map<TransactionDto, Transaction>(trans);
      if(validationResult.IsValid)
      {
        logger.LogInformation("Transaction is valid {TransactionName}", transaction.Name);
        _transactionRepository.Add(transaction);

        logger.LogInformation("Saving transaction {TransactionName}", transaction.Name);
        await _transactionRepository.Save();
      }
      else
      {
		    logger.LogInformation("Failed to create transaction. Please check it is valid {TransactionName}", transaction.Name);
        foreach(var error in validationResult.Errors)
        {
          logger.LogError(error.ErrorMessage);
        }
		    return ApiResponse.ValidationError<int>("Validation error");
      }

      
      return ApiResponse.Success<int>(transaction.Id, "Successfully created transaction");
    }
  }
}
