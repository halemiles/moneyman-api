using Api.Interfaces;
using Moneyman.Models;
using System.Collections.Generic;
using System.Linq;
using System;
using Moneyman.Domain;
using Moneyman.Interfaces;
using Moneyman.Services.Validators;

namespace Moneyman.Services
{
	public class TransactionService : ITransactionService
	{
		private readonly ITransactionRepository _transactionRepository;

		public TransactionService(ITransactionRepository transactionRepository)
		{
			_transactionRepository = transactionRepository;
		}

    public int Update(Transaction model, int Id)
    {
        TransactionValidator transactionValidator = new TransactionValidator();
        var validationResult = transactionValidator.Validate(model);

        if(validationResult.IsValid)
        {
          _transactionRepository.Update(model, Id);
          _transactionRepository.Save();
        }

      return model.Id;
    }

    public void Delete(int id)
    {
        //  TODO - Implement
        _transactionRepository.Remove(id);
        _transactionRepository.Save();
        return;
    }

    public List<Transaction> GetAll(int userId)
    {
      return _transactionRepository.GetAll().ToList();
    }

    public Transaction GetById(int userId, int id)
    {
      return _transactionRepository.Get(id);
    }
  }
}
