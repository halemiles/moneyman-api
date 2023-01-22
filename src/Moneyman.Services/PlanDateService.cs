using Moneyman.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System;
using Moneyman.Domain;
using Moneyman.Services.Validators;
using AutoMapper;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Moneyman.Services
{
	public class PlanDateService : IPlanDateService
	{
		private readonly IPlanDateRepository _planDateRepository;
    private readonly ILogger<PlanDateService> logger;

		public PlanDateService(
      IPlanDateRepository planDateRepository,
      ILogger<PlanDateService> logger
    )
		{
			_planDateRepository = planDateRepository;
      this.logger = logger;
		}

      public List<PlanDate> GetAll()
      {
          return _planDateRepository.GetAll().ToList();
      }

        public List<PlanDate> Search(string transactionName)
        {
            var planDates = _planDateRepository.GetAll();
            if(!string.IsNullOrEmpty(transactionName))
            {
              planDates = planDates.Where(x => x.Transaction.Name == transactionName);
            }
            return planDates.ToList();
        }
    }
}
