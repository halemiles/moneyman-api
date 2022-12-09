using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Logging;
using Moneyman.Interfaces;

namespace Moneyman.Services
{
    public class BaseService
    {
        protected readonly ILogger logger;

        public BaseService(
            ILogger logger
        )
        {
            this.logger = logger;
        }
    }   
}