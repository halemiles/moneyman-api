using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.Sqlite;
using Moneyman.Domain;

namespace Moneyman.Domain
{
    public class MoneymanContext : DbContext
    {        
        protected readonly IConfiguration Configuration;
        private bool IsTest { get; set; } 
        public MoneymanContext(DbContextOptions<MoneymanContext> options) : base(options)
        {
            IsTest = true;
        }
        public MoneymanContext(DbContextOptions<MoneymanContext> options, IConfiguration configuration) : base(options)
        {
            Configuration = configuration;
        }
        public MoneymanContext(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public MoneymanContext(){}
        public DbSet<Transaction> Transactions {get; set;}
       
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if(!IsTest)
            {
                options.UseSqlite(new SqliteConnection(Configuration.GetConnectionString("WebApiDatabase")));            
            }
            
        }

    }
}