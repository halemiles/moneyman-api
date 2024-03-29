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
        public MoneymanContext(DbContextOptions<MoneymanContext> options) : base(options) {}
        public MoneymanContext(){}
        public DbSet<Transaction> Transactions {get; set;}
        public DbSet<Payday> Paydays {get; set;}
        public DbSet<PlanDate> PlanDates {get; set;}
       
    }
}