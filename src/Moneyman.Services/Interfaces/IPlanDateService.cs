using System.Collections.Generic;
using Moneyman.Domain;

namespace Moneyman.Interfaces
{
	public interface IPlanDateService
	{
		List<PlanDate> GetAll();
	}
}
