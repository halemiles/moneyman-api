using Moneyman.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System;
using Moneyman.Domain;
using Moneyman.Services.Validators;
using AutoMapper;

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
				Id = i,
				Date = new DateTime(2021,i+1,dayOfMonth)
			};
			payDates.Add(pd);
		}

		return payDates;
    }
  }
}
