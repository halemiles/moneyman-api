using System.Collections.Generic;
using System.Threading.Tasks;
using Moneyman.Domain;
using Moneyman.Domain.Models;

namespace Moneyman.Interfaces
{
	public interface IBankAccountService
	{
		//Task<ApiResponse<int>> Create(TransactionDto trans);
		List<BankAccountDto> GetAll();
		// Transaction GetById(int id);
		// int Update(Transaction model);
		// void Update(List<Transaction> model);
    	// void Delete(int id);
	}
}
