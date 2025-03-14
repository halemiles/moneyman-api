using System;
using AutoMapper;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Moneyman.Domain;
using Moneyman.Domain.MapperProfiles;
using Moneyman.Domain.Settings;
using Moneyman.Interfaces;
using Moneyman.Persistence;
using Moneyman.Services;
using Moneyman.Services.Interfaces;
using Serilog;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IPaydayRepository, PaydayRepository>();
        services.AddScoped<IPlanDateRepository, PlanDateRepository>();
        services.AddScoped<IBankAccountRepository, BankAccountRepository>();

        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<IPaydayService, PaydayService>();
        services.AddScoped<IWeekdayService, WeekdayService>();
        services.AddScoped<IHolidayService, HolidayService>();
        services.AddScoped<IDtpService, DtpService>();
        services.AddScoped<IOffsetCalculationService, OffsetCalculationService>();
        services.AddScoped<IPlanDateService, PlanDateService>();
        services.AddScoped<IBankAccountService, BankAccountService>();
        services.AddScoped<IDateTimeProvider, DateTimeProvider>();

        return services;
    }
    public static void AddAutomapperProfiles(this IServiceCollection services)
    {
        AutoMapper.IConfigurationProvider config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<TransactionProfile>();
                cfg.AddProfile<TransactionDtoToTransactionProfile>();
                cfg.AddProfile<PlanDateDtoProfile>();
                cfg.AddProfile<BankAccountProfile>();
            });

            services.AddSingleton(config);
            services.AddScoped<IMapper, Mapper>();
    }

    public static IServiceCollection SetupLogger(this IServiceCollection services)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.Seq("http://localhost:5341")
            .CreateLogger();

        services.AddSingleton(Log.Logger);
        return services;
    }

    public static IServiceCollection SetupContexts(this IServiceCollection services, IConfiguration configuration)
    {
        try
        {
            services.AddDbContext<MoneymanContext>(
                options => options.UseSqlite(
                    new SqliteConnection(configuration.GetConnectionString("WebApiDatabase")),
                    x => x.MigrationsAssembly("Moneyman.Api")
                )
            );
        }
        catch(Exception err)
        {
            Console.WriteLine(err.ToString());
        }
        return services;
    }

    public static IServiceCollection SetupCors(this IServiceCollection services){
        services.AddCors(options =>
            {
                options.AddPolicy(name: "AllowAnyOrigin",
                    builder => {
                        builder
                        .AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                    });
            });
        return services;
    }

    public static IServiceCollection SetupHolidays(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<HolidayOptions>(configuration.GetSection("HolidayOptions"));
        return services;
    }

    public static IServiceCollection SetupSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Moneyman.Api", Version = "v1" });
        });
        return services;
    }
}