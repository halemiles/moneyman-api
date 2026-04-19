using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moneyman.Domain;
using Moneyman.Domain.Settings;
using Moneyman.Interfaces;

namespace Moneyman.Api.Extensions
{
    public class BankHolidayInitializerHostedService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IBankHolidayCache _cache;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<BankHolidayInitializerHostedService> _logger;
        private readonly BankHolidayOptions _options;

        public BankHolidayInitializerHostedService(
            IServiceProvider serviceProvider,
            IBankHolidayCache cache,
            IHttpClientFactory httpClientFactory,
            ILogger<BankHolidayInitializerHostedService> logger,
            IOptions<BankHolidayOptions> options)
        {
            _serviceProvider = serviceProvider;
            _cache = cache;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _options = options.Value;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Initialising bank holidays from gov.uk API");
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<IBankHolidayRepository>();

                var cutoff = DateTime.UtcNow.AddMonths(18).Date;
                var existing = repository.GetAll().Select(h => h.Date.Date).ToHashSet();

                var apiHolidays = await FetchFromApiAsync(cancellationToken);

                var toAdd = apiHolidays
                    .Where(h => h.Date.Date <= cutoff && !existing.Contains(h.Date.Date))
                    .ToList();

                foreach (var holiday in toAdd)
                    repository.Add(holiday);

                if (toAdd.Count > 0)
                {
                    await repository.Save();
                    _logger.LogInformation("Added {Count} bank holidays to the database", toAdd.Count);
                }

                var allHolidays = repository.GetAll()
                    .Select(h => h.Date.ToString("dd-MM-yyyy"))
                    .ToList();

                _cache.Populate(allHolidays);
                _logger.LogInformation("Bank holiday cache populated with {Count} entries", allHolidays.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialise bank holidays from gov.uk API");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        private async Task<List<BankHoliday>> FetchFromApiAsync(CancellationToken cancellationToken)
        {
            using var client = _httpClientFactory.CreateClient();
            var json = await client.GetStringAsync(_options.Url, cancellationToken);

            using var doc = JsonDocument.Parse(json);
            var events = doc.RootElement
                .GetProperty(_options.Region)
                .GetProperty("events");

            var holidays = new List<BankHoliday>();
            foreach (var evt in events.EnumerateArray())
            {
                var dateStr = evt.GetProperty("date").GetString();
                var title = evt.GetProperty("title").GetString();
                if (DateTime.TryParse(dateStr, out var date))
                    holidays.Add(new BankHoliday { Date = date.Date, Title = title });
            }
            return holidays;
        }
    }
}
