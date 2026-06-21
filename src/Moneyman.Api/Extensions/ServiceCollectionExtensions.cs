using System;

using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Moneyman.Api.Data;
using Moneyman.Domain;
using Moneyman.Domain.MapperProfiles;
using Moneyman.Domain.Settings;
using Moneyman.Interfaces;
using Moneyman.Persistence;
using Moneyman.Services;
using Moneyman.Services.Interfaces;
using Moneyman.Services.Validators;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IPaydayRepository, PaydayRepository>();
        services.AddScoped<IPlanDateRepository, PlanDateRepository>();
        services.AddScoped<IBankAccountRepository, BankAccountRepository>();
        services.AddScoped<IBankHolidayRepository, BankHolidayRepository>();

        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<IPaydayService, PaydayService>();
        services.AddScoped<IHolidayService, HolidayService>();
        services.AddScoped<IDtpService, DtpService>();
        services.AddScoped<IOffsetCalculationService, OffsetCalculationService>();
        services.AddScoped<IPlanDateService, PlanDateService>();
        services.AddScoped<IBankAccountService, BankAccountService>();
        services.AddScoped<IDateTimeProvider, DateTimeProvider>();
        services.AddScoped<IValidator<TransactionDto>, TransactionDtoValidator>();

        return services;
    }
    public static void AddMappers(this IServiceCollection services)
    {
        services.AddSingleton<BankAccountMapper>();
        services.AddSingleton<TransactionMapper>();
        services.AddSingleton<PlanDateMapper>();
    }

    public static IServiceCollection SetupContexts(this IServiceCollection services, IConfiguration configuration)
    {
        try
        {
            services.AddDbContext<MoneymanContext>(
                options => options
                    .UseSqlite(
                        configuration.GetConnectionString("WebApiDatabase"),
                        x => x.MigrationsAssembly("Moneyman.Api")
                    )
                    .AddInterceptors(new SqlitePragmaInterceptor())
            );
        }
        catch(Exception err)
        {
            Console.WriteLine(err.ToString());
        }
        return services;
    }

    public const string AllowAnyOriginPolicy = "AllowAnyOrigin";
    public const string RestrictedOriginsPolicy = "RestrictedOrigins";

    public static IServiceCollection SetupCors(this IServiceCollection services, IConfiguration configuration){
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

        services.AddCors(options =>
            {
                options.AddPolicy(name: AllowAnyOriginPolicy,
                    builder => {
                        builder
                        .AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                    });

                options.AddPolicy(name: RestrictedOriginsPolicy,
                    builder => {
                        if (allowedOrigins.Length > 0)
                        {
                            builder
                            .WithOrigins(allowedOrigins)
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                        }
                    });
            });
        return services;
    }

    public static IServiceCollection SetupHolidays(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<BankHolidayApiOptions>(configuration.GetSection("BankHolidayApiOptions"));
        services.AddSingleton<IBankHolidayCache, BankHolidayCache>();
        services.AddHttpClient();
        return services;
    }

    public static IServiceCollection SetupPayday(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<PaydayOptions>(configuration.GetSection("PaydayOptions"));
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
