using System.Collections.Generic;
using System.Threading.Tasks;
using Moneyman.Domain;
using Moneyman.Domain.Models;

namespace Moneyman.Interfaces
{
	public interface ITransactionService
	{
		Task<ApiResponse<int>> Create(TransactionDto trans);
		List<TransactionDto> GetAll();
		Transaction GetById(int id);
		int Update(Transaction model);
		void Update(List<Transaction> model);
    	void Delete(int id);
	}
}
