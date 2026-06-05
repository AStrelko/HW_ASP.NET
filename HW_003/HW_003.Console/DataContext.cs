using HW_003.Console.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace HW_003.Console;

public class DataContext: DbContext
{
    public DataContext()
    {
    }
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("config.json")
                .Build();

            optionsBuilder
                .UseSqlServer(config.GetConnectionString("Local"))
                .EnableSensitiveDataLogging()
                .LogTo(System.Console.WriteLine);
        }
    }
    
    public DbSet<Order> Orders { get; set; }
    public DbSet<BankTransaction> BankTransactions { get; set; }
    public DbSet<Log> Logs { get; set; }

}