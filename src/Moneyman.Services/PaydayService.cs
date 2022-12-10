using Moneyman.Interfaces;
using System.Collections.Generic;
using System;
using Moneyman.Domain;
using AutoMapper;
using Moneyman.Services.Interfaces;
using System.Linq;

namespace Moneyman.Services
{
	public class PaydayService : IPaydayService
	{
		private readonly IPaydayRepository _paydayRepository;
		private readonly IOffsetCalculationService _offsetCalculationService;
		private readonly IDateTimeProvider _dateTimeProvider;

		public PaydayService(
			IPaydayRepository paydayRepository,
			IOffsetCalculationService offsetCalculationService,
			IDateTimeProvider dateTimeProvider
		)
		{
			_paydayRepository = paydayRepository;
			_offsetCalculationService = offsetCalculationService;
			_dateTimeProvider = dateTimeProvider;

		}

		public List<Payday> Generate(int dayOfMonth)
		{
			_paydayRepository.RemoveAll("Paydays");
			List<Payday> payDates = new List<Payday>(); //TODO - Refactor this so we don't have to intialise
			for(int i=0;i<12;i++)
			{
				var plannedDate = new DateTime(DateTime.Now.Year,i+1,dayOfMonth);
				var offsetDate = _offsetCalculationService.CalculateOffset(plannedDate).PlanDate;
				Payday pd = new Payday
				{
					Date = offsetDate
				};
				payDates.Add(pd);
				_paydayRepository.Add(pd);
				_paydayRepository.Save();
			}
			
			return payDates;
		}

        public Payday GetNext()
        {
            return _paydayRepository
				.GetAll()
				.Where(x => x.Date > _dateTimeProvider.GetNow())
				.FirstOrDefault();

        }

        public Payday GetPrevious()
        {
            return _paydayRepository
				.GetAll()
				.Where(x => x.Date < _dateTimeProvider.GetNow())
				.LastOrDefault();
        }
    }
}
