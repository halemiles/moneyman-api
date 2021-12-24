using Moneyman.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System;
using Moneyman.Domain;
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
          _transactionRepository.Update(model);
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

    public List<Transaction> GetAll()
    {
      return _transactionRepository.GetAll().ToList();
    }

    public Transaction GetById(int id)
    {
      return _transactionRepository.Get(id);
    }
  }
}
