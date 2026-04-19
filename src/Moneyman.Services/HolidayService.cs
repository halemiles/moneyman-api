using System.Collections.Generic;
using System.Linq;
using Moneyman.Interfaces;

namespace Moneyman.Services
{
    public class HolidayService : IHolidayService
    {
        private readonly IBankHolidayCache _cache;
        private readonly IBankHolidayRepository _repository;
        private readonly object _populateCacheLock = new object();

        public HolidayService(
            IBankHolidayCache cache,
            IBankHolidayRepository repository)
        {
            _cache = cache;
            _repository = repository;
        }

        public List<string> GenerateHolidays()
        {
            if (_cache.Holidays.Any())
                return _cache.Holidays.ToList();

            lock (_populateCacheLock)
            {
                if (_cache.Holidays.Any())
                    return _cache.Holidays.ToList();

                var storedHolidays = _repository.GetAll()
                    .Select(h => h.Date.ToString("dd-MM-yyyy"))
                    .ToList();

                if (storedHolidays.Any())
                {
                    _cache.Populate(storedHolidays);
                    return storedHolidays;
                }
                
                return new List<string>();
            }
        }
    }
}
