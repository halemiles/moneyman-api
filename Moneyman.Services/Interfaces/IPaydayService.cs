using System.Collections.Generic;
using Moneyman.Domain;

namespace Moneyman.Services.Interfaces
{
	public interface IPaydayService
	{
		List<Payday> Generate(int dayOfMonth);
	}
}
