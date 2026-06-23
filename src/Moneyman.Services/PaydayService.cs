using Moneyman.Interfaces;
using System.Collections.Generic;
using System;
using Moneyman.Domain;
using Moneyman.Services.Interfaces;
using System.Linq;
using Moneyman.Services.Extentions;

namespace Moneyman.Services
{
	public class PaydayService : IPaydayService
	{
		private readonly IPaydayRepository _paydayRepository;
		private readonly IOffsetCalculationService _offsetCalculationService;
		private readonly IDateTimeProvider _dateTimeProvider;
		private const int TotalPaydayMonths = 24; // How many years we should generate dates for

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

		public List<Payday> GetAll()
		{
			return _paydayRepository.GetAll().ToList();
		}

		public List<Payday> Generate(int dayOfMonth)
		{
			_paydayRepository.RemoveAll("Paydays");
			List<Payday> payDates = new List<Payday>(); //TODO - Refactor this so we don't have to intialise
			for(int i=0;i<TotalPaydayMonths;i++)
			{
				var plannedDate = new DateTime(_dateTimeProvider.GetNow().Year,1,dayOfMonth);
				plannedDate = plannedDate.AddMonths(i);
				var offsetDate = _offsetCalculationService.CalculateOffset(plannedDate).PlanDate;
				Payday pd = new Payday
				{
					Date = offsetDate
				};
				payDates.Add(pd);
				_paydayRepository.Add(pd);
			}
			_paydayRepository.Save();

			return payDates;
		}

        public Payday GetNext()
        {
            return _paydayRepository
				.GetAll()
				.FirstOrDefault(x => x.Date > _dateTimeProvider.GetNow());

        }

        public Payday GetPrevious()
        {
            return _paydayRepository
				.GetAll()
				.LastOrDefault(x => x.Date < _dateTimeProvider.GetNow());
        }

        public void RemoveAll()
        {
	        _paydayRepository.RemoveAll("Paydays");
        }
    }
}
