using Moneyman.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System;
using Moneyman.Domain;
using Moneyman.Services.Validators;
using System.Threading.Tasks;

namespace Moneyman.Services
{
	public class PlanDateService : IPlanDateService
	{
		private readonly IPlanDateRepository _planDateRepository;

		public PlanDateService(
      IPlanDateRepository planDateRepository
    )
		{
			_planDateRepository = planDateRepository;
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

        public ApiResponse<PlanDate> MarkAsPaid(int id)
        {
            var planDate = _planDateRepository.Get(id);
            if (planDate == null)
            {
                return ApiResponse.NotFound<PlanDate>($"Plan date with id {id} not found");
            }

            planDate.Paid = true;
            _planDateRepository.Update(planDate);
            return ApiResponse.Success<PlanDate>(planDate, "Plan date marked as paid");
        }
    }
}
