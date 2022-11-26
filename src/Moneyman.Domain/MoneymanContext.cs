using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.Sqlite;
using Moneyman.Domain;
using System.Diagnostics.CodeAnalysis;

namespace Moneyman.Domain
{
    [ExcludeFromCodeCoverage]
    public class MoneymanContext : DbContext
    {        
        protected readonly IConfiguration Configuration;
        private bool IsTest { get; set; } 
        public MoneymanContext(DbContextOptions<MoneymanContext> options) : base(options)
        {
        }

        public MoneymanContext(){}
        public DbSet<Transaction> Transactions {get; set;}
        public DbSet<Payday> Paydays {get; set;}
        public DbSet<PlanDate> PlanDates {get; set;}
       
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            
        }

    }
}