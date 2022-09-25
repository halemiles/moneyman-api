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

            try
            {
                services.AddDbContext<MoneymanContext>(
                    options => options.UseSqlite(
                        new SqliteConnection(Configuration.GetConnectionString("WebApiDatabase")),
                        x => x.MigrationsAssembly("Moneyman.Api")
                    )
                    
                );
            }
            catch(Exception err)
            {
                Console.WriteLine(err.ToString());
            }

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Moneyman.Api", Version = "v1" });
            });

            services.AddScoped<ITransactionRepository, TransactionRepository>();
            services.AddScoped<ITransactionService, TransactionService>();
            services.AddScoped<IPaydayRepository, PaydayRepository>();
            services.AddScoped<IPlanDateRepository, PlanDateRepository>();
            services.AddScoped<IPaydayService, PaydayService>();
            services.AddScoped<IWeekdayService, WeekdayService>();
            services.AddScoped<IHolidayService, HolidayService>();
            services.AddScoped<IDtpService, DtpService>();
            
            AutoMapper.IConfigurationProvider config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<TransactionProfile>();
                cfg.AddProfile<TransactionToTransactionDtoProfile>();
                cfg.AddProfile<TransactionDtoToTransactionProfile>();
            });

            services.AddSingleton(config);
            services.AddScoped<IMapper, Mapper>();

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

            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
