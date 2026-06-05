using ConsoleAppLes3Lab.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ConsoleAppLes3Lab;


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

            optionsBuilder.UseSqlServer(
                config.GetConnectionString("Local")
            );
        }
    }
    public DbSet<Product> Products { get; set; }
    public DbSet<ActivLog> ActivLogs { get; set; }
}