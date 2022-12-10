using System.Collections.Generic;
using Moneyman.Domain;

namespace Moneyman.Interfaces
{
	public interface ITransactionService
	{
		bool Create(Transaction trans);
		List<Transaction> GetAll();
		Transaction GetById(int id);
		int Update(Transaction model);
		void Update(List<Transaction> model);
    	void Delete(int id);
	}
}
