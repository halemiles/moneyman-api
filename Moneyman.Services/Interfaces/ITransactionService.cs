using System.Collections.Generic;
using Moneyman.Domain;

namespace Api.Interfaces //TODO - Update this
{
	public interface ITransactionService
	{
		List<Transaction> GetAll(int userId);
		Transaction GetById(int userId, int id);
		int Update(Transaction model, int id);
    	void Delete(int id);
	}
}
