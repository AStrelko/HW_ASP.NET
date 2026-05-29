using Microsoft.EntityFrameworkCore;
using OnlineStoreLes02Lab.Core.Models;
using OnlineStoreLes02Lab.Storage;

namespace OnlineStoreLes02Lab.Storage;


public class DataContext : DbContext
{
    //для звязуваня з класом  Alt+Enter -> перша строка
    public DbSet<User> Users { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<ActiveLog> Logs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Server=localhost;Database=DBHW03;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
    }
    
}
//для міграціі
//NuGet пакети:
//Microsoft.EntityFrameworkCore   - .Storage
//Microsoft.EntityFrameworkCore.Tools   - .Storage
//Microsoft.EntityFrameworkCore.Design  - .Storage
//Microsoft.EntityFrameworkCore.SqlServer  - .Storage

//міграція:
//в Terminal перехожу в Storage -> cd OnlineStoreLes02Lab.Storage
//періверяю версіі ->  dotnet ef --version
// роблю міграцію ->  dotnet ef migrations add InitialCreate (повина зявитись папка міграції, а в терміналі Done)
//оновлюю БД -> dotnet ef database update  (в терміналі Done)










