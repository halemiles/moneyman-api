using Microsoft.Extensions.DependencyInjection;
using Moneyman.Interfaces;
using Moneyman.Persistence;
using Moneyman.Services;
using Moneyman.Services.Interfaces;

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

        return services;
    }
}