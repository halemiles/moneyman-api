using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moneyman.Services;
using Moneyman.Services.Interfaces;

namespace Moneyman.Api.Extensions
{
    public class PaydayInitializerHostedService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IOptions<PaydayOptions> _paydayOptions;
        private readonly ILogger<PaydayInitializerHostedService> _logger;

        public PaydayInitializerHostedService(
            IServiceProvider serviceProvider,
            IOptions<PaydayOptions> paydayOptions,
            ILogger<PaydayInitializerHostedService> logger)
        {
            _serviceProvider = serviceProvider;
            _paydayOptions = paydayOptions;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Initializing paydays with day of month: {DayOfMonth}", _paydayOptions.Value.DayOfMonth);
            
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var paydayService = scope.ServiceProvider.GetRequiredService<IPaydayService>();
                    var dayOfMonth = _paydayOptions.Value.DayOfMonth;
                    
                    if (dayOfMonth < 1 || dayOfMonth > 31)
                    {
                        _logger.LogError("Invalid DayOfMonth value: {DayOfMonth}. Must be between 1 and 31.", dayOfMonth);
                        return Task.CompletedTask;
                    }
                    
                    paydayService.Generate(dayOfMonth);
                    _logger.LogInformation("Paydays initialized successfully");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize paydays. Database may not be ready.");
            }
            
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
