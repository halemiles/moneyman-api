using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Sentry;

namespace Moneyman.Api.Middleware
{
    /// <summary>
    /// Tags the current Sentry scope with Playwright test context supplied via request
    /// headers. This lets a failing API test be traced to the exact server-side event in
    /// Sentry by searching for <c>test.name</c> / <c>test.run_id</c>.
    /// </summary>
    public class SentryTestContextMiddleware
    {
        public const string TestNameHeader = "X-Test-Name";
        public const string TestRunIdHeader = "X-Test-Run-Id";

        private readonly RequestDelegate _next;

        public SentryTestContextMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue(TestNameHeader, out var testName))
            {
                SentrySdk.ConfigureScope(scope => scope.SetTag("test.name", testName.ToString()));
            }

            if (context.Request.Headers.TryGetValue(TestRunIdHeader, out var runId))
            {
                SentrySdk.ConfigureScope(scope => scope.SetTag("test.run_id", runId.ToString()));
            }

            await _next(context);
        }
    }
}
