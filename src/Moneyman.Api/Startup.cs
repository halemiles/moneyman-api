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
using AutoMapper;
using Moneyman.Services.Interfaces;
using Serilog;

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
            services.AddAutomapperProfiles();
            services.SetupCors();
            services.SetupLogger();
            services.SetupHolidays(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Moneyman.Api v1"));
            }

            //TODO - Start using this
            //app.UseHttpsRedirection();

	        app.UseCors("AllowAnyOrigin");

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
