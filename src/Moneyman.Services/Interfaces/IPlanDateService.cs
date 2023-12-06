using System.Collections.Generic;
using Moneyman.Domain;

namespace Moneyman.Interfaces
{
	public interface IPlanDateService
	{
		/// <summary>
		/// Get all plan dates
		/// </summary>
		/// <returns></returns>
		List<PlanDate> GetAll();

		/// <summary>
		/// Search for plan dates
		/// </summary>
		/// <param name="transactionName"></param>
		/// <returns></returns>
		List<PlanDate> Search(string transactionName);
	}
}
