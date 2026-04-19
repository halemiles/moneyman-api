using System;
using System.Collections.Generic;
using System.Globalization;
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
using Moneyman.Interfaces;
using Moneyman.Services;

namespace Moneyman.Api.Extensions
{
    public class BankHolidayInitializerHostedService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IBankHolidayCache _cache;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<BankHolidayInitializerHostedService> _logger;
        private readonly IOptions<BankHolidayApiOptions> _bankHolidayApiOptions;

        public BankHolidayInitializerHostedService(
            IServiceProvider serviceProvider,
            IBankHolidayCache cache,
            IHttpClientFactory httpClientFactory,
            IOptions<BankHolidayApiOptions> bankHolidayApiOptions,
            ILogger<BankHolidayInitializerHostedService> logger)
        {
            _serviceProvider = serviceProvider;
            _cache = cache;
            _httpClientFactory = httpClientFactory;
            _bankHolidayApiOptions = bankHolidayApiOptions;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Initialising bank holidays from gov.uk API");
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<IBankHolidayRepository>();

                try
                {
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
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to initialise bank holidays from gov.uk API");
                }
                finally
                {
                    PopulateCacheFromDatabase(repository);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize bank holidays from database");
            }
        }

        private void PopulateCacheFromDatabase(IBankHolidayRepository repository)
        {
            var allHolidays = repository.GetAll()
                .Select(h => h.Date.ToString("dd-MM-yyyy"))
                .ToList();

            _cache.Populate(allHolidays);
            _logger.LogInformation("Bank holiday cache populated with {Count} entries", allHolidays.Count);
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        private async Task<List<BankHoliday>> FetchFromApiAsync(CancellationToken cancellationToken)
        {
            using var client = _httpClientFactory.CreateClient();
            var url = _bankHolidayApiOptions.Value.Url;
            var region = _bankHolidayApiOptions.Value.Region;
            var json = await client.GetStringAsync(url, cancellationToken);

            using var doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty(region, out var regionElement))
            {
                _logger.LogWarning("Configured bank holiday region '{Region}' was not found in API response", region);
                return new List<BankHoliday>();
            }

            if (!regionElement.TryGetProperty("events", out var events))
                return new List<BankHoliday>();

            var holidays = new List<BankHoliday>();
            foreach (var evt in events.EnumerateArray())
            {
                if (!evt.TryGetProperty("date", out var dateProperty) || !evt.TryGetProperty("title", out var titleProperty))
                    continue;

                var dateStr = dateProperty.GetString();
                var title = titleProperty.GetString();

                if (string.IsNullOrWhiteSpace(dateStr) || string.IsNullOrWhiteSpace(title))
                    continue;

                if (DateTime.TryParseExact(dateStr, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                    holidays.Add(new BankHoliday { Date = date.Date, Title = title });
            }
            return holidays;
        }
    }
}
