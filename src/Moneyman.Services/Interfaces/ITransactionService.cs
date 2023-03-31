using System.Collections.Generic;
using Moneyman.Domain;
using Moneyman.Domain.Models;

namespace Moneyman.Interfaces
{
	public interface ITransactionService
	{
		ApiResponse<int> Create(Transaction trans);
		List<TransactionDto> GetAll();
		Transaction GetById(int id);
		int Update(Transaction model);
		void Update(List<Transaction> model);
    	void Delete(int id);
	}
}
