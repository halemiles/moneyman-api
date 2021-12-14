using Api.Interfaces;
using Api.Data;
using Api.Models;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Api.Services
{
	public class TransactionService : ITransactionService
	{
		private readonly ITransactionRepository _transactionRepository;
		public TransactionService(ITransactionRepository transactionRepository)
		{
			_transactionRepository = transactionRepository;
		}

    public int Update(MoneyTransaction model, int Id)
    {
        _transactionRepository.Update(model, Id);
        _transactionRepository.Save();

      return Id;
    }

    public void Delete(int id)
    {
        //  TODO - Implement
        _transactionRepository.Remove(id);
        _transactionRepository.Save();
        return;
    }

    public List<MoneyTransaction> GetAll(int userId)
    {
      return _transactionRepository.GetAll().ToList();
    }

    public MoneyTransaction GetById(int userId, int id)
    {
      return _transactionRepository.Get(id);
    }
  }
}
