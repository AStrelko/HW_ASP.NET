using Microsoft.EntityFrameworkCore;
using less_005Pr.Async.Models;

namespace less_005Pr.Async;

using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(
            "Server=localhost;Database=lesson_05Pr;Trusted_Connection=True;TrustServerCertificate=True;");
    }
}