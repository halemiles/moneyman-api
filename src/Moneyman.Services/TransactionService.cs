using Moneyman.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System;
using Moneyman.Domain;
using Moneyman.Services.Validators;
using AutoMapper;
using System.Threading.Tasks;

namespace Moneyman.Services
{
	public class TransactionService : ITransactionService
	{
		private readonly ITransactionRepository _transactionRepository;

		public TransactionService(ITransactionRepository transactionRepository)
		{
			_transactionRepository = transactionRepository;
      
		}

    public int Update(Transaction model)
    {
       
      _transactionRepository.Update(model);
      _transactionRepository.Save();
     

      return model.Id;
    }

    public void Update(List<Transaction> model)
    {
      foreach(var transaction in model)
      {
        _transactionRepository.Update(transaction);
      }
      _transactionRepository.Save();
    }

    public void Delete(int id)
    {
        _transactionRepository.Remove(id);
        _transactionRepository.Save();
        return;
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
      var validationResult = transactionValidator.Validate(trans);

      if(validationResult.IsValid)
      {
        _transactionRepository.Add(trans);
        _transactionRepository.Save();
        return true;
      }
      return false;
    }
  }
}
