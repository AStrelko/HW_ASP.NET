using DictionaryApp.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace DictionaryApp.Storage;

public class DataContext : DbContext
{
    /*public DataContext(DbContextOptions<DataContext> options) : base(options)//по стандарту
    {         заважає створити міграцію
    }*/
    //як ми зєднуємось з БД
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Server=localhost;Database=DictionaryAppDB;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;");
    }
    
    //опис настройки таблиц та колонок
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //щоб не повтор. в обох таблицях
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<DeletedUser>()
            .HasIndex(u => u.Email)
            .IsUnique();
    }
    
    
    
    //створюю БД за допомогою міграцій 
    //в Terminal команда <<    dotnet ef migrations add " назва міграціі "   >>
    //оновити БД 
    //в Terminal команда <<    dotnet ef database update    >>
    
    public  DbSet<DictionaryItem> Dictionaries { get; set; }
    public  DbSet<User> Users { get; set; }
    public DbSet<DeletedUser> DeletedUsers { get; set; }
}