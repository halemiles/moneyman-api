using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using Moneyman.Domain;
using Moneyman.Domain.Settings;
using Moneyman.Interfaces;

namespace Moneyman.Services
{
    public class HolidayService : IHolidayService
    {
        private readonly IBankHolidayCache _cache;
        private readonly IBankHolidayRepository _repository;
        private readonly IOptions<HolidayOptions> _holidayOptions;
        private readonly object _cacheLock = new object();

        public HolidayService(
            IBankHolidayCache cache,
            IBankHolidayRepository repository,
            IOptions<HolidayOptions> holidayOptions)
        {
            _cache = cache;
            _repository = repository;
            _holidayOptions = holidayOptions;
        }

        public List<string> GenerateHolidays()
        {
            if (_cache.Holidays.Any())
                return _cache.Holidays.ToList();

            lock (_cacheLock)
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

                var configuredHolidays = _holidayOptions.Value?.Holidays?.ToList() ?? new List<string>();
                if (configuredHolidays.Any())
                    _cache.Populate(configuredHolidays);

                return configuredHolidays;
            }
        }
    }
}
