using Moneyman.Interfaces;
using System.Collections.Generic;
using System;
using Moneyman.Domain;
using AutoMapper;
using Moneyman.Services.Interfaces;

namespace Moneyman.Services
{
	public class PaydayService : IPaydayService
	{
		private readonly IMapper _mapper;
		private readonly IPaydayRepository _paydayRepository;

		public PaydayService(IPaydayRepository paydayRepository)
		{
			_paydayRepository = paydayRepository;
		}

		public List<Payday> Generate(int dayOfMonth)
		{
		
			List<Payday> payDates = new List<Payday>(); //TODO - Refactor this so we don't have to intialise
			for(int i=0;i<12;i++)
			{
				Payday pd = new Payday()
				{
					Date = new DateTime(2021,i+1,dayOfMonth)
				};
				payDates.Add(pd);
				_paydayRepository.Add(pd);
				_paydayRepository.Save();
			}
			
			return payDates;
		}
  	}
}
