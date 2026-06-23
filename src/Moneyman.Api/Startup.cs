using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Moneyman.Services;
using Moneyman.Interfaces;
using Moneyman.Persistence;
using Moneyman.Domain;
using Moneyman.Domain.MapperProfiles;
using Moneyman.Services.Interfaces;
using Moneyman.Api.Extensions;
using Moneyman.Api.Middleware;
namespace Moneyman.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApplicationInsightsTelemetry();
            services.SetupContexts(Configuration);
            services.AddControllers();
            services.SetupSwagger();
            services.AddRepositories();
            services.AddServices();
            services.AddMappers();
            services.SetupCors(Configuration);
            services.SetupHolidays(Configuration);
            services.SetupPayday(Configuration);
            services.AddHostedService<BankHolidayInitializerHostedService>();
            services.AddHostedService<PaydayInitializerHostedService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        // Configure must remain an instance method: the ASP.NET Startup convention invokes it on the Startup instance.
#pragma warning disable S2325
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
#pragma warning restore S2325
            // Runs after Sentry's own middleware (registered by UseSentry), so the
            // tags it sets land on the current request's Sentry scope.
            app.UseMiddleware<SentryTestContextMiddleware>();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Moneyman.Api v1"));
            }

	        app.UseCors(env.IsDevelopment()
	            ? Extensions.ServiceCollectionExtensions.AllowAnyOriginPolicy
	            : Extensions.ServiceCollectionExtensions.RestrictedOriginsPolicy);

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
