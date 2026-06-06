using Les04_pr.Con.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;


namespace Les04_pr.Con;

public class DataContext: DbContext
{
    public DataContext(){}
    public DataContext(DbContextOptions<DataContext> options) : base(options) { }

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
    
    public DbSet<Book> Books { get; set; }
}