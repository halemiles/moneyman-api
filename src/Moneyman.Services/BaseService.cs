using System;
using Microsoft.Extensions.Logging;

namespace Moneyman.Services
{
    public class BaseService
    {
        protected readonly ILogger logger;

        public BaseService(
            ILogger logger
        )
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
    }   
}