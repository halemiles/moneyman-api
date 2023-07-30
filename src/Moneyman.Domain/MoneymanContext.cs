using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moneyman.Domain;
using System.Diagnostics.CodeAnalysis;

namespace Moneyman.Domain
{
    [ExcludeFromCodeCoverage]
    public class MoneymanContext : DbContext
    {
        IConfiguration configuration;
        public MoneymanContext (IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        //public MoneymanContext(DbContextOptions<MoneymanContext> options) : base(options){}
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = configuration.GetConnectionString("Default");
            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        }
        public DbSet<Transaction> Transactions {get; set;}
        public DbSet<Payday> Paydays {get; set;}
        public DbSet<PlanDate> PlanDates {get; set;}
       
    }
}