using System.Collections.Generic;
using Moneyman.Domain;

namespace Moneyman.Interfaces
{
	public interface IPaydayService
	{
		List<Payday> Generate(int dayOfMonth);
	}
}
