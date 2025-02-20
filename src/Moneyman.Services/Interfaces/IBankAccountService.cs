using System.Collections.Generic;
using System.Threading.Tasks;
using Moneyman.Domain;
using Moneyman.Domain.Models;

namespace Moneyman.Interfaces
{
	public interface IBankAccountService
	{
		/// <summary>
		/// Get all bank accounts
		/// </summary>
		/// <returns></returns>
		List<BankAccountDto> GetAll();
	}
}
