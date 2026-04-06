using Moneyman.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System;
using Moneyman.Domain;
using Moneyman.Services.Validators;
using Moneyman.Domain.MapperProfiles;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moneyman.Domain.Models;
using Moneyman.Persistence;

namespace Moneyman.Services
{
	public class TransactionService : ITransactionService
	{
		private readonly ITransactionRepository _transactionRepository;
    private readonly ILogger<TransactionService> logger;

    private readonly TransactionMapper transactionMapper;

		public TransactionService(
      ITransactionRepository transactionRepository,
      ILogger<TransactionService> logger,
      TransactionMapper transactionMapper
    )
		{
			_transactionRepository = transactionRepository;
      this.logger = logger;
      this.transactionMapper = transactionMapper;
		}

    public int Update(Transaction model)
    {
        var existing = _transactionRepository.Get(model.Id); // _context.Set<Transaction>().AsNoTracking().FirstOrDefault(x => x.Id == entity.Id);

        if (existing == null)
        {
            return 0;
        }

        if(model.Name != existing.Name && !string.IsNullOrEmpty(model.Name))
        {
            existing.Name = model.Name;
        }

        if(model.Amount != existing.Amount && model.Amount > 0)
        {
            existing.Amount = model.Amount;
        }

        if(model.StartDate != existing.StartDate && model.StartDate != DateTime.MinValue)
        {
            existing.StartDate = model.StartDate;
        }

        if(model.Frequency != existing.Frequency)
        {
            existing.Frequency = model.Frequency;
        }

        if(model.Active != existing.Active)
        {
          existing.Active = model.Active;
        }

      _transactionRepository.Update(existing);
      _transactionRepository.Save();
        logger.LogInformation("Saving transaction {TransactionName}", model.Name);
        return 1;
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

    public void DeleteAll()
    {
      // Remove dependent plan dates via repository SQL and then transactions
      _transactionRepository.RemoveAll("PlanDates");
      _transactionRepository.RemoveAll("Transactions");
      logger.LogInformation("Deleting all transactions and plan dates");
      _transactionRepository.Save();
    }

    public List<TransactionDto> GetAll()
    {
      var transactions =  _transactionRepository.GetAll();
      var transactionsAsDto = transactions.Select(transactionMapper.ToDto).ToList();
      return transactionsAsDto;
    }

    public List<TransactionDto> GetAnticipated()
    {
      var transactions =  _transactionRepository.GetAll().Where(x => x.IsAnticipated);
      var transactionsAsDto = transactions.Select(transactionMapper.ToDto).ToList();
      return transactionsAsDto;
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
      var transaction = transactionMapper.ToEntity(trans);
      if(validationResult.IsValid)
      {
        try
        {
          logger.LogInformation("Transaction is valid {TransactionName}", transaction.Name);
          _transactionRepository.Add(transaction);

          logger.LogInformation("Saving transaction {TransactionName}", transaction.Name);
          await _transactionRepository.Save();
        }
        catch (Exception err)
        {
          logger.LogError("Failed to save transaction {TransactionName} {Error}", transaction.Name, err.Message);
          return ApiResponse.ValidationError<int>("Failed to save transaction");
        }
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
