using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moneyman.Domain;
using System.Diagnostics.CodeAnalysis;

namespace Moneyman.Domain
{
    [ExcludeFromCodeCoverage]
    public class MoneymanContext : DbContext
    {
        static readonly string connectionString = "Server=localhost;User ID=root;Password=specialpassword;Database=moneyman;SSL Mode=None";
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        }
        public DbSet<Transaction> Transactions {get; set;}
        public DbSet<Payday> Paydays {get; set;}
        public DbSet<PlanDate> PlanDates {get; set;}
       
    }
}