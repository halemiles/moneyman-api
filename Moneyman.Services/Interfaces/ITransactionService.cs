using System.Collections.Generic;
using Moneyman.Domain;

namespace Moneyman.Interfaces
{
	public interface ITransactionService
	{
		List<Transaction> GetAll();
		Transaction GetById(int id);
		int Update(Transaction model, int id);
    	void Delete(int id);
	}
}
