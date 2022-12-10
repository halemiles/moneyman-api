using Moneyman.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System;
using Moneyman.Domain;
using Moneyman.Services.Validators;
using AutoMapper;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Moneyman.Services
{
	public class TransactionService : ITransactionService
	{
		private readonly ITransactionRepository _transactionRepository;
    private readonly ILogger<TransactionService> logger;

		public TransactionService(
      ITransactionRepository transactionRepository,
      ILogger<TransactionService> logger
    )
		{
			_transactionRepository = transactionRepository;
      this.logger = logger;
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

    public List<Transaction> GetAll()
    {
      var transactions =  _transactionRepository.GetAll();
      return transactions?.ToList() ?? new List<Transaction>();
    }

    public Transaction GetById(int id)
    {
      return _transactionRepository.Get(id);
    }

    public bool Create(Transaction trans)
    {
      TransactionValidator transactionValidator = new TransactionValidator();
      logger.LogInformation("Validation transaction {TransactionName}", trans.Name);
      var validationResult = transactionValidator.Validate(trans);

      if(validationResult.IsValid)
      {
        logger.LogInformation("Transaction is valid {TransactionName}", trans.Name);
        _transactionRepository.Add(trans);
        logger.LogInformation("Saving transaction {TransactionName}", trans.Name);
        _transactionRepository.Save();
        return true;
      }

      logger.LogInformation("Failed to create transaction. Please check it is valid {TransactionName}", trans.Name);
      return false;
    }
  }
}
